using AutoMapper;
using Intervu.Application;
using Intervu.Application.Interfaces.ExternalServices;
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

namespace Intervu.API.Test.UnitTests.Application.UseCases.BookingRequest
{
    public class CancelInterviewRoundUseCaseTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_MultiRound_CancelsSelectedRoundAndProcessesRefund()
        {
            var candidateId = Guid.NewGuid();
            var coachId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var selectedRoundId = Guid.NewGuid();
            var otherRoundId = Guid.NewGuid();

            var selectedBlock = new CoachAvailability
            {
                Id = Guid.NewGuid(),
                CoachId = coachId,
                Status = CoachAvailabilityStatus.Booked,
                InterviewRoundId = selectedRoundId
            };

            var selectedRoom = new InterviewRoom
            {
                Id = Guid.NewGuid(),
                Status = InterviewRoomStatus.Scheduled
            };

            var booking = BuildBooking(
                bookingId,
                candidateId,
                coachId,
                new[]
                {
                    new InterviewRound
                    {
                        Id = selectedRoundId,
                        RoundNumber = 1,
                        StartTime = DateTime.UtcNow.AddDays(3),
                        EndTime = DateTime.UtcNow.AddDays(3).AddMinutes(60),
                        Price = 120,
                        Status = InterviewRoundStatus.Active,
                        InterviewRoom = selectedRoom,
                        AvailabilityBlocks = [selectedBlock]
                    },
                    new InterviewRound
                    {
                        Id = otherRoundId,
                        RoundNumber = 2,
                        StartTime = DateTime.UtcNow.AddDays(4),
                        EndTime = DateTime.UtcNow.AddDays(4).AddMinutes(60),
                        Price = 150,
                        Status = InterviewRoundStatus.Active,
                        InterviewRoom = new InterviewRoom
                        {
                            Id = Guid.NewGuid(),
                            Status = InterviewRoomStatus.Scheduled
                        }
                    }
                });

            var payment = new InterviewBookingTransaction
            {
                Id = Guid.NewGuid(),
                BookingRequestId = bookingId,
                Type = TransactionType.Payment,
                Amount = 270,
                Status = TransactionStatus.Paid,
                UserId = candidateId
            };

            var payout = new InterviewBookingTransaction
            {
                Id = Guid.NewGuid(),
                BookingRequestId = bookingId,
                Type = TransactionType.Payout,
                Amount = 200,
                Status = TransactionStatus.Created,
                UserId = coachId
            };

            var ctx = BuildContext(booking, payment, payout, refundAmount: 90);
            var useCase = ctx.Provider.GetRequiredService<ICancelInterviewRound>();

            var result = await useCase.ExecuteAsync(candidateId, bookingId, selectedRoundId);

            Assert.Equal(BookingRequestStatus.Accepted, result.Status);
            Assert.Equal(InterviewRoundStatus.Cancelled, booking.Rounds.First(r => r.Id == selectedRoundId).Status);
            Assert.Equal(InterviewRoundStatus.Active, booking.Rounds.First(r => r.Id == otherRoundId).Status);
            Assert.Equal(InterviewRoomStatus.Cancelled, selectedRoom.Status);
            Assert.Equal(CoachAvailabilityStatus.Available, selectedBlock.Status);
            Assert.Null(selectedBlock.InterviewRoundId);

            Assert.Single(ctx.RefundTransactions);
            var refundTx = ctx.RefundTransactions.Single();
            Assert.Equal(TransactionType.Refund, refundTx.Type);
            Assert.Equal(90, refundTx.Amount);
            Assert.Equal(TransactionStatus.Paid, refundTx.Status);

            ctx.PaymentService.Verify(x => x.CreateSpendOrderAsync(
                90,
                "REFUND",
                booking.Candidate.BankBinNumber,
                "1234567890"), Times.Once);

            ctx.TransactionRepo.Verify(x => x.UpdateAsync(It.Is<InterviewBookingTransaction>(t =>
                t.Id == refundTx.Id && t.Status == TransactionStatus.Paid)), Times.Once);

            ctx.TransactionRepo.Verify(x => x.UpdateAsync(It.Is<InterviewBookingTransaction>(t =>
                t.Id == payout.Id && t.Status == TransactionStatus.Cancel)), Times.Never);

            ctx.BookingRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_LastActiveRound_CancelsBookingAndPayout()
        {
            var candidateId = Guid.NewGuid();
            var coachId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var roundId = Guid.NewGuid();

            var room = new InterviewRoom
            {
                Id = Guid.NewGuid(),
                Status = InterviewRoomStatus.Scheduled
            };

            var booking = BuildBooking(
                bookingId,
                candidateId,
                coachId,
                new[]
                {
                    new InterviewRound
                    {
                        Id = roundId,
                        RoundNumber = 1,
                        StartTime = DateTime.UtcNow.AddDays(2),
                        EndTime = DateTime.UtcNow.AddDays(2).AddMinutes(45),
                        Price = 100,
                        Status = InterviewRoundStatus.Active,
                        InterviewRoom = room,
                        AvailabilityBlocks = []
                    }
                });

            var payment = new InterviewBookingTransaction
            {
                Id = Guid.NewGuid(),
                BookingRequestId = bookingId,
                Type = TransactionType.Payment,
                Amount = 100,
                Status = TransactionStatus.Paid,
                UserId = candidateId
            };

            var payout = new InterviewBookingTransaction
            {
                Id = Guid.NewGuid(),
                BookingRequestId = bookingId,
                Type = TransactionType.Payout,
                Amount = 80,
                Status = TransactionStatus.Created,
                UserId = coachId
            };

            var ctx = BuildContext(booking, payment, payout, refundAmount: 75);
            var useCase = ctx.Provider.GetRequiredService<ICancelInterviewRound>();

            var result = await useCase.ExecuteAsync(candidateId, bookingId, roundId);

            Assert.Equal(BookingRequestStatus.Cancelled, result.Status);
            Assert.Equal(InterviewRoomStatus.Cancelled, room.Status);
            Assert.Single(ctx.RefundTransactions);
            Assert.Equal(TransactionStatus.Paid, ctx.RefundTransactions[0].Status);

            ctx.TransactionRepo.Verify(x => x.UpdateAsync(It.Is<InterviewBookingTransaction>(t =>
                t.Id == payout.Id && t.Status == TransactionStatus.Cancel)), Times.Once);

            ctx.PaymentService.Verify(x => x.CreateSpendOrderAsync(
                75,
                "REFUND",
                booking.Candidate.BankBinNumber,
                "1234567890"), Times.Once);

            ctx.BookingRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        private static RoundCancelContext BuildContext(
            DomainBookingRequest booking,
            InterviewBookingTransaction payment,
            InterviewBookingTransaction? payout,
            int refundAmount)
        {
            var bookingRepo = new Mock<IBookingRequestRepository>();
            var roomRepo = new Mock<IInterviewRoomRepository>();
            var transactionRepo = new Mock<ITransactionRepository>();
            var availabilityRepo = new Mock<ICoachAvailabilitiesRepository>();
            var refundPolicy = new Mock<IRefundPolicy>();
            var paymentService = new Mock<IPaymentService>();
            var bankFieldProtector = new Mock<IBankFieldProtector>();

            var refundTransactions = new List<InterviewBookingTransaction>();

            bookingRepo.Setup(x => x.GetByIdWithDetailsAsync(booking.Id)).ReturnsAsync(booking);
            bookingRepo.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            transactionRepo.Setup(x => x.GetByBookingRequestId(booking.Id, TransactionType.Payment)).ReturnsAsync(payment);
            transactionRepo.Setup(x => x.GetByBookingRequestId(booking.Id, TransactionType.Payout)).ReturnsAsync(payout);
            transactionRepo.Setup(x => x.AddAsync(It.IsAny<InterviewBookingTransaction>()))
                .Callback<InterviewBookingTransaction>(tx => refundTransactions.Add(tx))
                .Returns(Task.CompletedTask);

            refundPolicy.Setup(x => x.CalculateRefundAmount(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(refundAmount);

            paymentService.Setup(x => x.CreateSpendOrderAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(true);

            bankFieldProtector
                .Setup(x => x.Decrypt(booking.Candidate.BankAccountNumber))
                .Returns("1234567890");

            var serviceCollection = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["ApiClients:AIService"] = "https://example.com" })
                .Build();

            serviceCollection.AddUseCases(config);
            serviceCollection.AddSingleton<IMapper>(new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())));
            serviceCollection.AddScoped(_ => bookingRepo.Object);
            serviceCollection.AddScoped(_ => roomRepo.Object);
            serviceCollection.AddScoped(_ => transactionRepo.Object);
            serviceCollection.AddScoped(_ => availabilityRepo.Object);
            serviceCollection.AddScoped(_ => refundPolicy.Object);
            serviceCollection.AddScoped(_ => paymentService.Object);
            serviceCollection.AddScoped(_ => bankFieldProtector.Object);

            var provider = serviceCollection.BuildServiceProvider();
            return new RoundCancelContext(
                provider,
                bookingRepo,
                transactionRepo,
                paymentService,
                refundTransactions);
        }

        private static DomainBookingRequest BuildBooking(
            Guid bookingId,
            Guid candidateId,
            Guid coachId,
            IEnumerable<InterviewRound> rounds)
        {
            var roundList = rounds.ToList();

            return new DomainBookingRequest
            {
                Id = bookingId,
                CandidateId = candidateId,
                CoachId = coachId,
                Type = BookingRequestType.JDInterview,
                Status = BookingRequestStatus.Accepted,
                TotalAmount = roundList.Sum(r => r.Price),
                Rounds = roundList,
                Candidate = new CandidateProfile
                {
                    Id = candidateId,
                    BankBinNumber = "970418",
                    BankAccountNumber = "enc-account",
                    BankAccountNumberMasked = "******7890",
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

        private sealed record RoundCancelContext(
            ServiceProvider Provider,
            Mock<IBookingRequestRepository> BookingRepo,
            Mock<ITransactionRepository> TransactionRepo,
            Mock<IPaymentService> PaymentService,
            List<InterviewBookingTransaction> RefundTransactions);
    }
}
