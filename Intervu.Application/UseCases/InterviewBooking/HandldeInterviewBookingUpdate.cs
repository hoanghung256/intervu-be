using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Application.UseCases.InterviewRoom;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Hosting;
using System.Transactions;

namespace Intervu.Application.UseCases.InterviewBooking
{
    public class HandldeInterviewBookingUpdate : IHandldeInterviewBookingUpdate
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPaymentService _paymentService;
        private readonly ICoachAvailabilitiesRepository _coachAvailabilitiesRepository;
        private readonly IBackgroundService _backgroundService;
        private readonly ICreateInterviewRoom _createInterviewRoom;
        private readonly IInterviewRoomRepository _interviewRoomRepository;

        public HandldeInterviewBookingUpdate(ITransactionRepository transactionRepository, IPaymentService paymentService, ICoachAvailabilitiesRepository coachAvailabilitiesRepository, IBackgroundService backgroundService, ICreateInterviewRoom createInterviewRoom) 
        {
            _transactionRepository = transactionRepository;
            _paymentService = paymentService;
            _coachAvailabilitiesRepository = coachAvailabilitiesRepository;
            _backgroundService = backgroundService;
            _createInterviewRoom = createInterviewRoom;
        }

        public async Task ExecuteAsync(object webhookPayload)
        {
            //if (webhookPayload is not Webhook payloadCasting) return null;
            var (isValid, orderCode) = _paymentService.VerifyPayment(webhookPayload);

            if (!isValid) return;

            InterviewBookingTransaction transaction = await _transactionRepository.GetByOrderCode(orderCode) ?? throw new Exception("Booking transaction not found");

            CoachAvailability availability = await _coachAvailabilitiesRepository.GetByIdAsync(transaction.CoachAvailabilityId) ?? throw new Exception("Coach availability not found");

            if (availability.Status == CoachAvailabilityStatus.Available)
            {
                throw new Exception("Coach availability is not reserved");
            }

            if (transaction.UserId != availability.ReservingForUserId || availability.ReservingForUserId == null)
            {
                throw new Exception("Transaction user does not match the reserving user");
            }

            availability.Status = CoachAvailabilityStatus.Booked;
            await _coachAvailabilitiesRepository.SaveChangesAsync();

            transaction.Status = Domain.Entities.Constants.TransactionStatus.Paid;
            await _transactionRepository.SaveChangesAsync();

            // TODO: Create Interview Room include avai Id (for reschedule purpose)
            _backgroundService.Enqueue<ICreateInterviewRoom>(
                uc => uc.ExecuteAsync(transaction.UserId, availability.CoachId, availability.StartTime)
            );

            // TODO: Notify candidate and coach
        }
    }
}
