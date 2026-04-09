using AutoMapper;
using Intervu.Application;
using Intervu.Application.DTOs.BookingRequest;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.BookingRequest;
using Intervu.Application.Mappings;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Linq.Expressions;
using DomainBookingRequest = Intervu.Domain.Entities.BookingRequest;

namespace Intervu.API.Test.UnitTests.Application.UseCases.BookingRequest
{
    public class RespondToBookingRequestUseCaseTests
    {
        // --------------- NORMAL PATHS ---------------

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_ApprovePaidBooking_SetsAcceptedAndQueuesRooms()
        {
            var coachId = Guid.NewGuid();
            var candidateId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var roundId = Guid.NewGuid();
            var blockId = Guid.NewGuid();

            var block = new CoachAvailability { Id = blockId, StartTime = DateTime.UtcNow.AddDays(3) };
            var booking = BuildPaidBooking(bookingId, candidateId, coachId, serviceId, [
                new InterviewRound
                {
                    Id = roundId, RoundNumber = 1,
                    StartTime = DateTime.UtcNow.AddDays(3),
                    EndTime = DateTime.UtcNow.AddDays(3).AddHours(1),
                    CoachInterviewServiceId = serviceId,
                    CoachInterviewService = new CoachInterviewService { Id = serviceId, DurationMinutes = 60, CoachId = coachId },
                    AvailabilityBlocks = [block]
                }
            ]);

            var payment = new InterviewBookingTransaction
            {
                Id = Guid.NewGuid(), BookingRequestId = bookingId,
                Type = TransactionType.Payment, Amount = 100, Status = TransactionStatus.Paid,
                UserId = candidateId
            };

            var ctx = BuildContext(booking, payment, payout: null);
            var useCase = ctx.Provider.GetRequiredService<IRespondToBookingRequest>();

            var result = await useCase.ExecuteAsync(coachId, bookingId,
                new RespondToBookingRequestDto { IsApproved = true });

            Assert.Equal(BookingRequestStatus.Accepted, result.Status);
            Assert.NotNull(result.RespondedAt);

            // One room enqueued per round
            ctx.BackgroundService.Verify(
                x => x.Enqueue<Intervu.Application.Interfaces.UseCases.InterviewRoom.ICreateInterviewRoom>(
                    It.IsAny<Expression<Action<Intervu.Application.Interfaces.UseCases.InterviewRoom.ICreateInterviewRoom>>>()),
                Times.Once);

            // Candidate notified
            ctx.BackgroundService.Verify(
                x => x.Enqueue<Intervu.Application.Interfaces.UseCases.Notification.INotificationUseCase>(
                    It.IsAny<Expression<Action<Intervu.Application.Interfaces.UseCases.Notification.INotificationUseCase>>>()),
                Times.Once);

            ctx.BookingRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_RejectPaidBooking_SetsRejectedAndIssuesFullRefund()
        {
            var coachId = Guid.NewGuid();
            var candidateId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var roundId = Guid.NewGuid();

            var block = new CoachAvailability
            {
                Id = Guid.NewGuid(), CoachId = coachId,
                Status = CoachAvailabilityStatus.Booked, InterviewRoundId = roundId
            };
            var booking = BuildPaidBooking(bookingId, candidateId, coachId, serviceId, [
                new InterviewRound
                {
                    Id = roundId, RoundNumber = 1,
                    StartTime = DateTime.UtcNow.AddDays(3),
                    EndTime = DateTime.UtcNow.AddDays(3).AddHours(1),
                    CoachInterviewServiceId = serviceId,
                    AvailabilityBlocks = [block]
                }
            ]);

            var payment = new InterviewBookingTransaction
            {
                Id = Guid.NewGuid(), BookingRequestId = bookingId,
                Type = TransactionType.Payment, Amount = 200, Status = TransactionStatus.Paid,
                UserId = candidateId
            };
            var payout = new InterviewBookingTransaction
            {
                Id = Guid.NewGuid(), BookingRequestId = bookingId,
                Type = TransactionType.Payout, Amount = 200, Status = TransactionStatus.Created,
                UserId = coachId
            };

            var ctx = BuildContext(booking, payment, payout);
            var useCase = ctx.Provider.GetRequiredService<IRespondToBookingRequest>();

            var result = await useCase.ExecuteAsync(coachId, bookingId,
                new RespondToBookingRequestDto { IsApproved = false, RejectionReason = "Not available" });

            Assert.Equal(BookingRequestStatus.Rejected, result.Status);
            Assert.Equal("Not available", booking.RejectionReason);

            // Payout cancelled
            Assert.Equal(TransactionStatus.Cancel, payout.Status);

            // 100% refund created
            ctx.TransactionRepo.Verify(x => x.AddAsync(It.Is<InterviewBookingTransaction>(t =>
                t.Type == TransactionType.Refund &&
                t.Amount == 200 &&
                t.UserId == candidateId)), Times.Once);

            // Availability block freed
            Assert.Equal(CoachAvailabilityStatus.Available, block.Status);
            Assert.Null(block.InterviewRoundId);
            ctx.AvailabilityRepo.Verify(x => x.UpdateAsync(It.IsAny<CoachAvailability>()), Times.Once);

            // Candidate notified
            ctx.BackgroundService.Verify(
                x => x.Enqueue<Intervu.Application.Interfaces.UseCases.Notification.INotificationUseCase>(
                    It.IsAny<Expression<Action<Intervu.Application.Interfaces.UseCases.Notification.INotificationUseCase>>>()),
                Times.Once);

            ctx.BookingRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        // --------------- BOUNDARY CASES ---------------

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_ApproveWithNoPaymentTransaction_QueuesRoomWithNullTransactionId()
        {
            var coachId = Guid.NewGuid();
            var candidateId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();

            var booking = BuildPaidBooking(bookingId, candidateId, coachId, serviceId, [
                new InterviewRound
                {
                    Id = Guid.NewGuid(), RoundNumber = 1,
                    StartTime = DateTime.UtcNow.AddDays(3),
                    EndTime = DateTime.UtcNow.AddDays(3).AddHours(1),
                    CoachInterviewServiceId = serviceId,
                    CoachInterviewService = new CoachInterviewService { Id = serviceId, DurationMinutes = 60, CoachId = coachId },
                    AvailabilityBlocks = []
                }
            ]);

            var ctx = BuildContext(booking, payment: null, payout: null);
            var useCase = ctx.Provider.GetRequiredService<IRespondToBookingRequest>();

            var result = await useCase.ExecuteAsync(coachId, bookingId,
                new RespondToBookingRequestDto { IsApproved = true });

            Assert.Equal(BookingRequestStatus.Accepted, result.Status);

            // Room still queued even without transaction
            ctx.BackgroundService.Verify(
                x => x.Enqueue<Intervu.Application.Interfaces.UseCases.InterviewRoom.ICreateInterviewRoom>(
                    It.IsAny<Expression<Action<Intervu.Application.Interfaces.UseCases.InterviewRoom.ICreateInterviewRoom>>>()),
                Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_RejectWithNoPayout_CreatesRefundWithoutCrash()
        {
            var coachId = Guid.NewGuid();
            var candidateId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();

            var booking = BuildPaidBooking(bookingId, candidateId, coachId, serviceId, []);

            var payment = new InterviewBookingTransaction
            {
                Id = Guid.NewGuid(), BookingRequestId = bookingId,
                Type = TransactionType.Payment, Amount = 150, Status = TransactionStatus.Paid,
                UserId = candidateId
            };

            var ctx = BuildContext(booking, payment, payout: null);
            var useCase = ctx.Provider.GetRequiredService<IRespondToBookingRequest>();

            var result = await useCase.ExecuteAsync(coachId, bookingId,
                new RespondToBookingRequestDto { IsApproved = false, RejectionReason = "Conflict" });

            Assert.Equal(BookingRequestStatus.Rejected, result.Status);
            ctx.TransactionRepo.Verify(x => x.AddAsync(It.Is<InterviewBookingTransaction>(t =>
                t.Type == TransactionType.Refund && t.Amount == 150)), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_ExpiredPaidBooking_MarksExpiredAndThrows()
        {
            var coachId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            var booking = BuildPaidBooking(bookingId, Guid.NewGuid(), coachId, Guid.NewGuid(), []);
            booking.ExpiresAt = DateTime.UtcNow.AddSeconds(-1);

            var ctx = BuildContext(booking, null, null);
            var useCase = ctx.Provider.GetRequiredService<IRespondToBookingRequest>();

            await Assert.ThrowsAsync<BadRequestException>(
                () => useCase.ExecuteAsync(coachId, bookingId,
                    new RespondToBookingRequestDto { IsApproved = true }));

            Assert.Equal(BookingRequestStatus.Expired, booking.Status);
            ctx.BookingRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        // --------------- ABNORMAL / EXCEPTION PATHS ---------------

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_RespondToPendingBooking_ThrowsBadRequestException()
        {
            var coachId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            var booking = BuildPaidBooking(bookingId, Guid.NewGuid(), coachId, Guid.NewGuid(), []);
            booking.Status = BookingRequestStatus.Pending;

            var ctx = BuildContext(booking, null, null);
            var useCase = ctx.Provider.GetRequiredService<IRespondToBookingRequest>();

            await Assert.ThrowsAsync<BadRequestException>(
                () => useCase.ExecuteAsync(coachId, bookingId,
                    new RespondToBookingRequestDto { IsApproved = true }));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_WrongCoach_ThrowsForbiddenException()
        {
            var coachId = Guid.NewGuid();
            var wrongCoachId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            var booking = BuildPaidBooking(bookingId, Guid.NewGuid(), coachId, Guid.NewGuid(), []);

            var ctx = BuildContext(booking, null, null);
            var useCase = ctx.Provider.GetRequiredService<IRespondToBookingRequest>();

            await Assert.ThrowsAsync<ForbiddenException>(
                () => useCase.ExecuteAsync(wrongCoachId, bookingId,
                    new RespondToBookingRequestDto { IsApproved = true }));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_BookingNotFound_ThrowsNotFoundException()
        {
            var bookingRepo = new Mock<IBookingRequestRepository>();
            bookingRepo.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<Guid>())).ReturnsAsync((DomainBookingRequest?)null);

            var serviceCollection = BuildBaseServiceCollection();
            serviceCollection.AddScoped(_ => bookingRepo.Object);
            serviceCollection.AddScoped(_ => new Mock<ITransactionRepository>().Object);
            serviceCollection.AddScoped(_ => new Mock<ICoachAvailabilitiesRepository>().Object);
            serviceCollection.AddScoped(_ => new Mock<ICoachInterviewServiceRepository>().Object);
            serviceCollection.AddScoped(_ => new Mock<IBackgroundService>().Object);

            var useCase = serviceCollection.BuildServiceProvider().GetRequiredService<IRespondToBookingRequest>();

            await Assert.ThrowsAsync<NotFoundException>(
                () => useCase.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(),
                    new RespondToBookingRequestDto { IsApproved = true }));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_RejectWithoutReason_ThrowsBadRequestException()
        {
            var coachId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            var booking = BuildPaidBooking(bookingId, Guid.NewGuid(), coachId, Guid.NewGuid(), []);

            var ctx = BuildContext(booking, null, null);
            var useCase = ctx.Provider.GetRequiredService<IRespondToBookingRequest>();

            await Assert.ThrowsAsync<BadRequestException>(
                () => useCase.ExecuteAsync(coachId, bookingId,
                    new RespondToBookingRequestDto { IsApproved = false, RejectionReason = "" }));
        }

        // --------------- HELPERS ---------------

        private static DomainBookingRequest BuildPaidBooking(
            Guid bookingId, Guid candidateId, Guid coachId, Guid serviceId,
            IEnumerable<InterviewRound> rounds)
        {
            return new DomainBookingRequest
            {
                Id = bookingId,
                CandidateId = candidateId,
                CoachId = coachId,
                Type = BookingRequestType.JDInterview,
                Status = BookingRequestStatus.Paid,
                TotalAmount = 200,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                Rounds = rounds.ToList(),
                Candidate = new CandidateProfile
                {
                    Id = candidateId,
                    User = new User { Id = candidateId, FullName = "Candidate Unit", Email = "c@test.com", Password = "pwd" }
                },
                Coach = new CoachProfile
                {
                    Id = coachId,
                    User = new User { Id = coachId, FullName = "Coach Unit", Email = "coach@test.com", Password = "pwd" }
                }
            };
        }

        private static RespondContext BuildContext(
            DomainBookingRequest booking,
            InterviewBookingTransaction? payment,
            InterviewBookingTransaction? payout)
        {
            var bookingRepo = new Mock<IBookingRequestRepository>();
            var transactionRepo = new Mock<ITransactionRepository>();
            var availabilityRepo = new Mock<ICoachAvailabilitiesRepository>();
            var serviceRepo = new Mock<ICoachInterviewServiceRepository>();
            var backgroundService = new Mock<IBackgroundService>();

            bookingRepo.Setup(x => x.GetByIdWithDetailsAsync(booking.Id)).ReturnsAsync(booking);
            bookingRepo.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            transactionRepo.Setup(x => x.GetByBookingRequestId(booking.Id, TransactionType.Payment)).ReturnsAsync(payment);
            transactionRepo.Setup(x => x.GetByBookingRequestId(booking.Id, TransactionType.Payout)).ReturnsAsync(payout);
            transactionRepo.Setup(x => x.AddAsync(It.IsAny<InterviewBookingTransaction>())).Returns(Task.CompletedTask);

            serviceRepo.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new CoachInterviewService
                {
                    Id = Guid.NewGuid(),
                    DurationMinutes = 60,
                    InterviewType = new InterviewType
                    {
                        Name = "Tech",
                        IsCoding = false,
                        EvaluationStructure = [new EvaluationItem { Type = "Technical", Question = "Q1" }]
                    }
                });

            var serviceCollection = BuildBaseServiceCollection();
            serviceCollection.AddScoped(_ => bookingRepo.Object);
            serviceCollection.AddScoped(_ => transactionRepo.Object);
            serviceCollection.AddScoped(_ => availabilityRepo.Object);
            serviceCollection.AddScoped(_ => serviceRepo.Object);
            serviceCollection.AddScoped(_ => backgroundService.Object);

            var provider = serviceCollection.BuildServiceProvider();
            return new RespondContext(provider, bookingRepo, transactionRepo, availabilityRepo, backgroundService);
        }

        private static ServiceCollection BuildBaseServiceCollection()
        {
            var serviceCollection = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["ApiClients:AIService"] = "https://example.com" })
                .Build();
            serviceCollection.AddUseCases(config);
            serviceCollection.AddSingleton<IMapper>(
                new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())));
            return serviceCollection;
        }

        private sealed record RespondContext(
            ServiceProvider Provider,
            Mock<IBookingRequestRepository> BookingRepo,
            Mock<ITransactionRepository> TransactionRepo,
            Mock<ICoachAvailabilitiesRepository> AvailabilityRepo,
            Mock<IBackgroundService> BackgroundService);
    }
}
