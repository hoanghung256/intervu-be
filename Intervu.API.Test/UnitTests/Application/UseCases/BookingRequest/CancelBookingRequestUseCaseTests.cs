using AutoMapper;
using Intervu.Application;
using Intervu.Application.Interfaces.UseCases.BookingRequest;
using Intervu.Application.Mappings;
using Intervu.Domain.Abstractions.Policies.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using DomainBookingRequest = Intervu.Domain.Entities.BookingRequest;
using DomainInterviewRoom = Intervu.Domain.Entities.InterviewRoom;

namespace Intervu.API.Test.UnitTests.Application.UseCases.BookingRequest
{
    public class CancelBookingRequestUseCaseTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_SingleRound_CancelsBookingAndRestoresAvailability()
        {
            var candidateId = Guid.NewGuid();
            var coachId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var roundId = Guid.NewGuid();

            var availabilityBlock = new CoachAvailability
            {
                Id = Guid.NewGuid(),
                CoachId = coachId,
                Status = CoachAvailabilityStatus.Booked,
                InterviewRoundId = roundId
            };

            var booking = BuildBooking(
                bookingId,
                candidateId,
                coachId,
                serviceId,
                new[]
                {
                    new InterviewRound
                    {
                        Id = roundId,
                        RoundNumber = 1,
                        StartTime = DateTime.UtcNow.AddDays(3),
                        EndTime = DateTime.UtcNow.AddDays(3).AddMinutes(60),
                        Price = 150,
                        AvailabilityBlocks = [availabilityBlock]
                    }
                });

            var room = new InterviewRoom { Id = Guid.NewGuid(), Status = InterviewRoomStatus.Scheduled };
            var payment = new InterviewBookingTransaction
            {
                Id = Guid.NewGuid(),
                BookingRequestId = bookingId,
                Type = TransactionType.Payment,
                Amount = 150,
                Status = TransactionStatus.Paid,
                UserId = candidateId
            };
            var payout = new InterviewBookingTransaction
            {
                Id = Guid.NewGuid(),
                BookingRequestId = bookingId,
                Type = TransactionType.Payout,
                Amount = 100,
                Status = TransactionStatus.Created,
                UserId = coachId
            };

            var context = BuildServiceProviderForCancel(
                booking,
                [room],
                payment,
                payout,
                refundAmount: 120);

            var useCase = context.Provider.GetRequiredService<ICancelBookingRequest>();

            var result = await useCase.ExecuteAsync(candidateId, bookingId);

            Assert.Equal(BookingRequestStatus.Cancelled, result.Status);
            Assert.Equal(CoachAvailabilityStatus.Available, availabilityBlock.Status);
            Assert.Null(availabilityBlock.InterviewRoundId);
            Assert.Equal(InterviewRoomStatus.Cancelled, room.Status);
            Assert.Equal("Candidate Unit", result.CandidateName);
            Assert.Equal("Coach Unit", result.CoachName);

            context.TransactionRepo.Verify(x => x.AddAsync(It.Is<InterviewBookingTransaction>(t =>
                t.Type == TransactionType.Refund && t.Amount == 120 && t.UserId == candidateId)), Times.Once);
            context.TransactionRepo.Verify(x => x.UpdateAsync(It.Is<InterviewBookingTransaction>(t =>
                t.Id == payout.Id && t.Status == TransactionStatus.Cancel)), Times.Once);
            context.AvailabilityRepo.Verify(x => x.UpdateAsync(It.IsAny<CoachAvailability>()), Times.Once);
            context.RoomRepo.Verify(x => x.UpdateAsync(It.Is<InterviewRoom>(r => r.Id == room.Id && r.Status == InterviewRoomStatus.Cancelled)), Times.Once);
            context.BookingRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_MultipleRounds_CancelsAllRoundsAndSkipsAlreadyCancelledRoom()
        {
            var candidateId = Guid.NewGuid();
            var coachId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var service1Id = Guid.NewGuid();
            var service2Id = Guid.NewGuid();
            var round1Id = Guid.NewGuid();
            var round2Id = Guid.NewGuid();

            var round1Blocks = new[]
            {
                new CoachAvailability { Id = Guid.NewGuid(), CoachId = coachId, Status = CoachAvailabilityStatus.Booked, InterviewRoundId = round1Id },
                new CoachAvailability { Id = Guid.NewGuid(), CoachId = coachId, Status = CoachAvailabilityStatus.Booked, InterviewRoundId = round1Id }
            };
            var round2Blocks = new[]
            {
                new CoachAvailability { Id = Guid.NewGuid(), CoachId = coachId, Status = CoachAvailabilityStatus.Booked, InterviewRoundId = round2Id }
            };

            var booking = BuildBooking(
                bookingId,
                candidateId,
                coachId,
                service1Id,
                new[]
                {
                    new InterviewRound
                    {
                        Id = round1Id,
                        RoundNumber = 1,
                        StartTime = DateTime.UtcNow.AddDays(4),
                        EndTime = DateTime.UtcNow.AddDays(4).AddMinutes(60),
                        Price = 100,
                        CoachInterviewServiceId = service1Id,
                        AvailabilityBlocks = round1Blocks
                    },
                    new InterviewRound
                    {
                        Id = round2Id,
                        RoundNumber = 2,
                        StartTime = DateTime.UtcNow.AddDays(4).AddHours(2),
                        EndTime = DateTime.UtcNow.AddDays(4).AddHours(2).AddMinutes(30),
                        Price = 80,
                        CoachInterviewServiceId = service2Id,
                        AvailabilityBlocks = round2Blocks
                    }
                });

            var rooms = new List<InterviewRoom>
            {
                new() { Id = Guid.NewGuid(), Status = InterviewRoomStatus.Scheduled },
                new() { Id = Guid.NewGuid(), Status = InterviewRoomStatus.Cancelled }
            };

            var payment = new InterviewBookingTransaction
            {
                Id = Guid.NewGuid(),
                BookingRequestId = bookingId,
                Type = TransactionType.Payment,
                Amount = 180,
                Status = TransactionStatus.Paid,
                UserId = candidateId
            };

            var context = BuildServiceProviderForCancel(
                booking,
                rooms,
                payment,
                payout: null,
                refundAmount: 180);

            var useCase = context.Provider.GetRequiredService<ICancelBookingRequest>();

            var result = await useCase.ExecuteAsync(candidateId, bookingId);

            Assert.Equal(BookingRequestStatus.Cancelled, result.Status);
            Assert.All(round1Blocks, b =>
            {
                Assert.Equal(CoachAvailabilityStatus.Available, b.Status);
                Assert.Null(b.InterviewRoundId);
            });
            Assert.All(round2Blocks, b =>
            {
                Assert.Equal(CoachAvailabilityStatus.Available, b.Status);
                Assert.Null(b.InterviewRoundId);
            });
            Assert.Equal(InterviewRoomStatus.Cancelled, rooms[0].Status);
            Assert.Equal(InterviewRoomStatus.Cancelled, rooms[1].Status);

            context.TransactionRepo.Verify(x => x.AddAsync(It.Is<InterviewBookingTransaction>(t =>
                t.Type == TransactionType.Refund && t.Amount == 180)), Times.Once);
            context.RoomRepo.Verify(x => x.UpdateAsync(It.IsAny<InterviewRoom>()), Times.Once);
            context.AvailabilityRepo.Verify(x => x.UpdateAsync(It.IsAny<CoachAvailability>()), Times.Exactly(3));
            context.BookingRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_PendingBookingWithNoTransactions_CancelsWithoutRefund()
        {
            var candidateId = Guid.NewGuid();
            var coachId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            var booking = BuildBooking(bookingId, candidateId, coachId, Guid.NewGuid(), []);
            booking.Status = BookingRequestStatus.Pending;

            var context = BuildServiceProviderForCancel(booking, [], payment: null, payout: null, refundAmount: 0);

            var useCase = context.Provider.GetRequiredService<ICancelBookingRequest>();
            var result = await useCase.ExecuteAsync(candidateId, bookingId);

            Assert.Equal(BookingRequestStatus.Cancelled, result.Status);
            context.TransactionRepo.Verify(x => x.AddAsync(It.IsAny<InterviewBookingTransaction>()), Times.Never);
            context.BookingRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_WrongCandidate_ThrowsForbiddenException()
        {
            var candidateId = Guid.NewGuid();
            var differentCandidateId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            var booking = BuildBooking(bookingId, candidateId, Guid.NewGuid(), Guid.NewGuid(), []);

            var context = BuildServiceProviderForCancel(booking, [], null, null, 0);
            var useCase = context.Provider.GetRequiredService<ICancelBookingRequest>();

            await Assert.ThrowsAsync<Intervu.Application.Exceptions.ForbiddenException>(
                () => useCase.ExecuteAsync(differentCandidateId, bookingId));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_BookingNotFound_ThrowsNotFoundException()
        {
            var bookingRepo = new Mock<IBookingRequestRepository>();
            bookingRepo.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<Guid>())).ReturnsAsync((DomainBookingRequest?)null);
            bookingRepo.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            var serviceCollection = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["ApiClients:AIService"] = "https://example.com" })
                .Build();
            serviceCollection.AddUseCases(config);
            serviceCollection.AddSingleton<IMapper>(new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())));
            serviceCollection.AddScoped(_ => bookingRepo.Object);
            serviceCollection.AddScoped(_ => new Mock<IInterviewRoomRepository>().Object);
            serviceCollection.AddScoped(_ => new Mock<ITransactionRepository>().Object);
            serviceCollection.AddScoped(_ => new Mock<ICoachAvailabilitiesRepository>().Object);
            serviceCollection.AddScoped(_ => new Mock<IRefundPolicy>().Object);

            var useCase = serviceCollection.BuildServiceProvider().GetRequiredService<ICancelBookingRequest>();
            await Assert.ThrowsAsync<Intervu.Application.Exceptions.NotFoundException>(
                () => useCase.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_RejectedStatus_ThrowsBadRequestException()
        {
            var candidateId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            var booking = BuildBooking(bookingId, candidateId, Guid.NewGuid(), Guid.NewGuid(), []);
            booking.Status = BookingRequestStatus.Rejected;

            var context = BuildServiceProviderForCancel(booking, [], null, null, 0);
            var useCase = context.Provider.GetRequiredService<ICancelBookingRequest>();

            await Assert.ThrowsAsync<Intervu.Application.Exceptions.BadRequestException>(
                () => useCase.ExecuteAsync(candidateId, bookingId));
        }

        private static CancelContext BuildServiceProviderForCancel(
            DomainBookingRequest booking,
            List<DomainInterviewRoom> rooms,
            InterviewBookingTransaction? payment,
            InterviewBookingTransaction? payout,
            int refundAmount)
        {
            var bookingRepo = new Mock<IBookingRequestRepository>();
            var roomRepo = new Mock<IInterviewRoomRepository>();
            var transactionRepo = new Mock<ITransactionRepository>();
            var availabilityRepo = new Mock<ICoachAvailabilitiesRepository>();
            var refundPolicy = new Mock<IRefundPolicy>();

            bookingRepo.Setup(x => x.GetByIdWithDetailsAsync(booking.Id)).ReturnsAsync(booking);
            bookingRepo.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            roomRepo.Setup(x => x.GetByBookingRequestIdAsync(booking.Id)).ReturnsAsync(rooms);

            transactionRepo.Setup(x => x.GetByBookingRequestId(booking.Id, TransactionType.Payment))
                .ReturnsAsync(payment);
            transactionRepo.Setup(x => x.GetByBookingRequestId(booking.Id, TransactionType.Payout))
                .ReturnsAsync(payout);
            transactionRepo.Setup(x => x.AddAsync(It.IsAny<InterviewBookingTransaction>())).Returns(Task.CompletedTask);

            refundPolicy.Setup(x => x.CalculateRefundAmount(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(refundAmount);

            var serviceCollection = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ApiClients:AIService"] = "https://example.com"
                })
                .Build();

            serviceCollection.AddUseCases(config);
            serviceCollection.AddSingleton<IMapper>(new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())));
            serviceCollection.AddScoped(_ => bookingRepo.Object);
            serviceCollection.AddScoped(_ => roomRepo.Object);
            serviceCollection.AddScoped(_ => transactionRepo.Object);
            serviceCollection.AddScoped(_ => availabilityRepo.Object);
            serviceCollection.AddScoped(_ => refundPolicy.Object);

            var provider = serviceCollection.BuildServiceProvider();
            return new CancelContext(provider, bookingRepo, roomRepo, transactionRepo, availabilityRepo);
        }

        private static DomainBookingRequest BuildBooking(
            Guid bookingId,
            Guid candidateId,
            Guid coachId,
            Guid defaultServiceId,
            IEnumerable<InterviewRound> rounds)
        {
            var roundList = rounds.ToList();
            foreach (var round in roundList)
            {
                if (round.CoachInterviewServiceId == Guid.Empty)
                {
                    round.CoachInterviewServiceId = defaultServiceId;
                }

                round.CoachInterviewService = new CoachInterviewService
                {
                    Id = round.CoachInterviewServiceId,
                    CoachId = coachId,
                    Price = round.Price,
                    DurationMinutes = (int)(round.EndTime - round.StartTime).TotalMinutes,
                    InterviewType = new InterviewType
                    {
                        Name = "Type",
                        IsCoding = false
                    }
                };
            }

            return new DomainBookingRequest
            {
                Id = bookingId,
                CandidateId = candidateId,
                CoachId = coachId,
                Type = BookingRequestType.JDInterview,
                Status = BookingRequestStatus.PendingForApprovalAfterPayment,
                TotalAmount = roundList.Sum(r => r.Price),
                Rounds = roundList,
                Candidate = new CandidateProfile
                {
                    Id = candidateId,
                    User = new User
                    {
                        Id = candidateId,
                        FullName = "Candidate Unit",
                        Email = "candidate@test.com",
                        Password = "pwd"
                    }
                },
                Coach = new CoachProfile
                {
                    Id = coachId,
                    User = new User
                    {
                        Id = coachId,
                        FullName = "Coach Unit",
                        Email = "coach@test.com",
                        Password = "pwd"
                    }
                }
            };
        }

        private sealed record CancelContext(
            ServiceProvider Provider,
            Mock<IBookingRequestRepository> BookingRepo,
            Mock<IInterviewRoomRepository> RoomRepo,
            Mock<ITransactionRepository> TransactionRepo,
            Mock<ICoachAvailabilitiesRepository> AvailabilityRepo);
    }
}
