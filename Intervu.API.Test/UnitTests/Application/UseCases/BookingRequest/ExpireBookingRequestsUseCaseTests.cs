using Intervu.Application;
using Intervu.Application.Interfaces.ExternalServices.Email;
using Intervu.Application.Interfaces.UseCases.BookingRequest;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using DomainBookingRequest = Intervu.Domain.Entities.BookingRequest;

namespace Intervu.API.Test.UnitTests.Application.UseCases.BookingRequest
{
    public class ExpireBookingRequestsUseCaseTests
    {
        // --------------- BOUNDARY CASES ---------------

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_NoExpiredRequests_ReturnsZero()
        {
            var ctx = BuildContext(pendingExpired: [], paidExpired: []);
            var useCase = ctx.Provider.GetRequiredService<IExpireBookingRequests>();

            var result = await useCase.ExecuteAsync();

            Assert.Equal(0, result);
            ctx.BookingRepo.Verify(x => x.SaveChangesAsync(), Times.Never);
            ctx.EmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        // --------------- NORMAL PATHS ---------------

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_PendingExpired_MarksExpiredWithoutRefundOrEmail()
        {
            var booking = BuildPendingExpiredBooking();

            var ctx = BuildContext(pendingExpired: [booking], paidExpired: []);
            var useCase = ctx.Provider.GetRequiredService<IExpireBookingRequests>();

            var result = await useCase.ExecuteAsync();

            Assert.Equal(1, result);
            Assert.Equal(BookingRequestStatus.Expired, booking.Status);
            ctx.TransactionRepo.Verify(x => x.AddAsync(It.IsAny<InterviewBookingTransaction>()), Times.Never);
            ctx.EmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
            ctx.BookingRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_PaidExpired_MarksExpiredRefundsFreeBlocksEmailsBoth()
        {
            var candidateId = Guid.NewGuid();
            var coachId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var roundId = Guid.NewGuid();

            var block = new CoachAvailability
            {
                Id = Guid.NewGuid(), CoachId = coachId,
                Status = CoachAvailabilityStatus.Booked, InterviewRoundId = roundId
            };

            var booking = BuildPaidExpiredBooking(bookingId, candidateId, coachId, roundId, [block]);

            var payment = new InterviewBookingTransaction
            {
                Id = Guid.NewGuid(), BookingRequestId = bookingId,
                Type = TransactionType.Payment, Amount = 300, Status = TransactionStatus.Paid,
                UserId = candidateId
            };
            var payout = new InterviewBookingTransaction
            {
                Id = Guid.NewGuid(), BookingRequestId = bookingId,
                Type = TransactionType.Payout, Amount = 300, Status = TransactionStatus.Created,
                UserId = coachId
            };

            var ctx = BuildContext(pendingExpired: [], paidExpired: [booking], payment, payout);
            var useCase = ctx.Provider.GetRequiredService<IExpireBookingRequests>();

            var result = await useCase.ExecuteAsync();

            Assert.Equal(1, result);
            Assert.Equal(BookingRequestStatus.Expired, booking.Status);

            // Payout cancelled
            Assert.Equal(TransactionStatus.Cancel, payout.Status);

            // 100% refund created
            ctx.TransactionRepo.Verify(x => x.AddAsync(It.Is<InterviewBookingTransaction>(t =>
                t.Type == TransactionType.Refund &&
                t.Amount == 300 &&
                t.UserId == candidateId)), Times.Once);

            // Availability block freed
            Assert.Equal(CoachAvailabilityStatus.Available, block.Status);
            Assert.Null(block.InterviewRoundId);
            ctx.AvailabilityRepo.Verify(x => x.UpdateAsync(It.IsAny<CoachAvailability>()), Times.Once);

            // Both emails sent
            ctx.EmailService.Verify(x => x.SendEmailAsync("c@test.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
            ctx.EmailService.Verify(x => x.SendEmailAsync("coach@test.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);

            ctx.BookingRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_PaidExpiredWithZeroAmount_NoRefundCreated()
        {
            var candidateId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            var booking = BuildPaidExpiredBooking(bookingId, candidateId, Guid.NewGuid(), Guid.NewGuid(), []);

            var payment = new InterviewBookingTransaction
            {
                Id = Guid.NewGuid(), BookingRequestId = bookingId,
                Type = TransactionType.Payment, Amount = 0, Status = TransactionStatus.Paid,
                UserId = candidateId
            };

            var ctx = BuildContext([], [booking], payment, payout: null);
            var useCase = ctx.Provider.GetRequiredService<IExpireBookingRequests>();

            await useCase.ExecuteAsync();

            ctx.TransactionRepo.Verify(x => x.AddAsync(It.IsAny<InterviewBookingTransaction>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_PaidExpiredWithNoCandidateEmail_OnlyCoachEmailSent()
        {
            var bookingId = Guid.NewGuid();
            var booking = BuildPaidExpiredBooking(bookingId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), []);
            // Clear candidate email
            booking.Candidate!.User!.Email = null!;

            var ctx = BuildContext([], [booking]);
            var useCase = ctx.Provider.GetRequiredService<IExpireBookingRequests>();

            await useCase.ExecuteAsync();

            ctx.EmailService.Verify(x => x.SendEmailAsync("coach@test.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
            ctx.EmailService.Verify(x => x.SendEmailAsync(null!, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        // --------------- HELPERS ---------------

        private static DomainBookingRequest BuildPendingExpiredBooking()
        {
            return new DomainBookingRequest
            {
                Id = Guid.NewGuid(),
                CandidateId = Guid.NewGuid(),
                CoachId = Guid.NewGuid(),
                Status = BookingRequestStatus.Pending,
                ExpiresAt = DateTime.UtcNow.AddHours(-1)
            };
        }

        private static DomainBookingRequest BuildPaidExpiredBooking(
            Guid bookingId, Guid candidateId, Guid coachId, Guid roundId,
            IEnumerable<CoachAvailability> blocks)
        {
            return new DomainBookingRequest
            {
                Id = bookingId,
                CandidateId = candidateId,
                CoachId = coachId,
                Status = BookingRequestStatus.Paid,
                ExpiresAt = DateTime.UtcNow.AddHours(-1),
                Rounds = [
                    new InterviewRound { Id = roundId, RoundNumber = 1, AvailabilityBlocks = blocks.ToList() }
                ],
                Candidate = new CandidateProfile
                {
                    Id = candidateId,
                    User = new User { Id = candidateId, FullName = "Candidate", Email = "c@test.com", Password = "pwd" }
                },
                Coach = new CoachProfile
                {
                    Id = coachId,
                    User = new User { Id = coachId, FullName = "Coach", Email = "coach@test.com", Password = "pwd" }
                }
            };
        }

        private static ExpireContext BuildContext(
            IEnumerable<DomainBookingRequest> pendingExpired,
            IEnumerable<DomainBookingRequest> paidExpired,
            InterviewBookingTransaction? payment = null,
            InterviewBookingTransaction? payout = null)
        {
            var bookingRepo = new Mock<IBookingRequestRepository>();
            var transactionRepo = new Mock<ITransactionRepository>();
            var availabilityRepo = new Mock<ICoachAvailabilitiesRepository>();
            var emailService = new Mock<IEmailService>();

            bookingRepo.Setup(x => x.GetExpiredPendingRequestsAsync()).ReturnsAsync(pendingExpired.ToList());
            bookingRepo.Setup(x => x.GetExpiredPaidRequestsAsync()).ReturnsAsync(paidExpired.ToList());
            bookingRepo.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Wire up payment/payout lookups for any booking that has them
            foreach (var booking in paidExpired)
            {
                var bid = booking.Id;
                transactionRepo.Setup(x => x.GetByBookingRequestId(bid, TransactionType.Payment)).ReturnsAsync(payment);
                transactionRepo.Setup(x => x.GetByBookingRequestId(bid, TransactionType.Payout)).ReturnsAsync(payout);
            }

            transactionRepo.Setup(x => x.AddAsync(It.IsAny<InterviewBookingTransaction>())).Returns(Task.CompletedTask);
            emailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            var serviceCollection = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["ApiClients:AIService"] = "https://example.com" })
                .Build();
            serviceCollection.AddUseCases(config);
            serviceCollection.AddScoped(_ => bookingRepo.Object);
            serviceCollection.AddScoped(_ => transactionRepo.Object);
            serviceCollection.AddScoped(_ => availabilityRepo.Object);
            serviceCollection.AddScoped(_ => emailService.Object);

            var provider = serviceCollection.BuildServiceProvider();
            return new ExpireContext(provider, bookingRepo, transactionRepo, availabilityRepo, emailService);
        }

        private sealed record ExpireContext(
            ServiceProvider Provider,
            Mock<IBookingRequestRepository> BookingRepo,
            Mock<ITransactionRepository> TransactionRepo,
            Mock<ICoachAvailabilitiesRepository> AvailabilityRepo,
            Mock<IEmailService> EmailService);
    }
}
