using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.AspNetCore;
using System.Transactions;

namespace Intervu.Application.UseCases.InterviewBooking
{
    public class HandldeInterviewBookingUpdate : IHandldeInterviewBookingUpdate
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPaymentService _paymentService;
        private readonly ICoachAvailabilitiesRepository _coachAvailabilitiesRepository;

        public HandldeInterviewBookingUpdate(ITransactionRepository transactionRepository, IPaymentService paymentService, ICoachAvailabilitiesRepository coachAvailabilitiesRepository) 
        {
            _transactionRepository = transactionRepository;
            _paymentService = paymentService;
            _coachAvailabilitiesRepository = coachAvailabilitiesRepository;
        }

        public async Task ExecuteAsync(object webhookPayload)
        {
            //if (webhookPayload is not Webhook payloadCasting) return null;
            var (isValid, orderCode) = _paymentService.VerifyPayment(webhookPayload);

            if (!isValid) return;

            InterviewBookingTransaction transaction = await _transactionRepository.GetByOrderCode(orderCode) ?? throw new Exception("Booking transaction not found");

            transaction.Status = Domain.Entities.Constants.TransactionStatus.Paid;
            await _transactionRepository.SaveChangesAsync();

            CoachAvailability availability = await _coachAvailabilitiesRepository.GetByIdAsync(transaction.CoachAvailabilityId) ?? throw new Exception("Coach availability not found");

            availability.Status = CoachAvailabilityStatus.Booked;
            await _coachAvailabilitiesRepository.SaveChangesAsync();

            // TODO: Notify candidate and coach
        }
    }
}
