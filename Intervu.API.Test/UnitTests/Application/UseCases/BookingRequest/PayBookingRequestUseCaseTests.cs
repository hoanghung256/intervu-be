using Intervu.Application;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.BookingRequest;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using DomainBookingRequest = Intervu.Domain.Entities.BookingRequest;

namespace Intervu.API.Test.UnitTests.Application.UseCases.BookingRequest
{
    public class PayBookingRequestUseCaseTests
    {
        // --------------- NORMAL PATHS ---------------

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_PendingBookingWithAmount_ReturnsCheckoutUrl()
        {
            var candidateId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var expectedUrl = "https://payos.vn/checkout/abc";

            var booking = BuildPendingBooking(bookingId, candidateId, totalAmount: 150);

            var ctx = BuildContext(booking, checkoutUrl: expectedUrl);
            var useCase = ctx.Provider.GetRequiredService<IPayBookingRequest>();

            var result = await useCase.ExecuteAsync(candidateId, bookingId, "https://return.url");

            Assert.Equal(expectedUrl, result);
            Assert.Equal(BookingRequestStatus.Pending, booking.Status); // stays Pending until webhook
            ctx.PaymentService.Verify(x => x.CreatePaymentOrderAsync(
                It.IsAny<int>(), 150, It.IsAny<string>(), "https://return.url", It.IsAny<long>()),
                Times.Once);
            ctx.UnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_FreeBooking_MarksBookingPaidSetsExpiresAtReturnsNull()
        {
            var candidateId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            var booking = BuildPendingBooking(bookingId, candidateId, totalAmount: 0);

            var capturedTransactions = new List<InterviewBookingTransaction>();
            var ctx = BuildContext(booking, capturedTransactions: capturedTransactions);
            var useCase = ctx.Provider.GetRequiredService<IPayBookingRequest>();

            var before = DateTime.UtcNow;
            var result = await useCase.ExecuteAsync(candidateId, bookingId, "https://return.url");
            var after = DateTime.UtcNow;

            Assert.Null(result);
            Assert.Equal(BookingRequestStatus.Paid, booking.Status);

            // ExpiresAt reset to now + 48h (coach response window)
            Assert.NotNull(booking.ExpiresAt);
            Assert.InRange(booking.ExpiresAt!.Value,
                before.AddHours(47).AddMinutes(59),
                after.AddHours(48).AddMinutes(1));

            // Both transactions marked Paid
            Assert.Equal(2, capturedTransactions.Count);
            Assert.All(capturedTransactions, tx => Assert.Equal(TransactionStatus.Paid, tx.Status));

            // No payment service called for free booking
            ctx.PaymentService.Verify(x => x.CreatePaymentOrderAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()),
                Times.Never);
        }

        // --------------- ABNORMAL / EXCEPTION PATHS ---------------

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_NonPendingBooking_ThrowsBadRequestException()
        {
            var candidateId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            var booking = BuildPendingBooking(bookingId, candidateId, totalAmount: 100);
            booking.Status = BookingRequestStatus.Paid; // already paid

            var ctx = BuildContext(booking);
            var useCase = ctx.Provider.GetRequiredService<IPayBookingRequest>();

            await Assert.ThrowsAsync<BadRequestException>(
                () => useCase.ExecuteAsync(candidateId, bookingId, "https://return.url"));

            ctx.UnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_WrongCandidate_ThrowsForbiddenException()
        {
            var candidateId = Guid.NewGuid();
            var wrongCandidateId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            var booking = BuildPendingBooking(bookingId, candidateId, totalAmount: 100);

            var ctx = BuildContext(booking);
            var useCase = ctx.Provider.GetRequiredService<IPayBookingRequest>();

            await Assert.ThrowsAsync<ForbiddenException>(
                () => useCase.ExecuteAsync(wrongCandidateId, bookingId, "https://return.url"));

            ctx.UnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_BookingNotFound_ThrowsNotFoundException()
        {
            var ctx = BuildContext(booking: null);
            var useCase = ctx.Provider.GetRequiredService<IPayBookingRequest>();

            await Assert.ThrowsAsync<NotFoundException>(
                () => useCase.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), "https://return.url"));

            ctx.UnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
        }

        // --------------- HELPERS ---------------

        private static DomainBookingRequest BuildPendingBooking(Guid bookingId, Guid candidateId, int totalAmount)
        {
            return new DomainBookingRequest
            {
                Id = bookingId,
                CandidateId = candidateId,
                CoachId = Guid.NewGuid(),
                Type = BookingRequestType.JDInterview,
                Status = BookingRequestStatus.Pending,
                TotalAmount = totalAmount,
                Rounds = []
            };
        }

        private static PayContext BuildContext(
            DomainBookingRequest? booking,
            string? checkoutUrl = null,
            List<InterviewBookingTransaction>? capturedTransactions = null)
        {
            var bookingRepo = new Mock<IBookingRequestRepository>();
            var transactionRepo = new Mock<ITransactionRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var paymentService = new Mock<IPaymentService>();

            if (booking != null)
                bookingRepo.Setup(x => x.GetByIdWithDetailsAsync(booking.Id)).ReturnsAsync(booking);
            else
                bookingRepo.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<Guid>())).ReturnsAsync((DomainBookingRequest?)null);

            transactionRepo.Setup(x => x.AddAsync(It.IsAny<InterviewBookingTransaction>()))
                .Callback<InterviewBookingTransaction>(tx => capturedTransactions?.Add(tx))
                .Returns(Task.CompletedTask);

            unitOfWork.Setup(x => x.GetRepository<IBookingRequestRepository>()).Returns(bookingRepo.Object);
            unitOfWork.Setup(x => x.GetRepository<ITransactionRepository>()).Returns(transactionRepo.Object);
            unitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            unitOfWork.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);
            unitOfWork.Setup(x => x.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            paymentService.Setup(x => x.CreatePaymentOrderAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .ReturnsAsync(checkoutUrl ?? "https://payos.vn/checkout/default");

            var serviceCollection = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["ApiClients:AIService"] = "https://example.com" })
                .Build();
            serviceCollection.AddUseCases(config);
            serviceCollection.AddLogging();
            serviceCollection.AddScoped(_ => unitOfWork.Object);
            serviceCollection.AddScoped(_ => paymentService.Object);

            var provider = serviceCollection.BuildServiceProvider();
            return new PayContext(provider, unitOfWork, paymentService);
        }

        private sealed record PayContext(
            ServiceProvider Provider,
            Mock<IUnitOfWork> UnitOfWork,
            Mock<IPaymentService> PaymentService);
    }
}
