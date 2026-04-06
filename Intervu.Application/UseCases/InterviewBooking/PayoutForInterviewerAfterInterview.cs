using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.InterviewBooking
{
    public class PayoutForCoachAfterInterview : IPayoutForCoachAfterInterview
    {
        private readonly IInterviewRoomRepository _interviewRoomRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICoachProfileRepository _coachProfileRepository;
        private readonly IPaymentService _paymentService;
        private readonly IBackgroundService _jobService;

        public PayoutForCoachAfterInterview(
            IInterviewRoomRepository interviewRoomRepository,
            ITransactionRepository transactionRepository,
            ICoachProfileRepository coachProfileRepository,
            IPaymentService paymentService,
            IBackgroundService jobService)
        {
            _interviewRoomRepository = interviewRoomRepository;
            _transactionRepository = transactionRepository;
            _coachProfileRepository = coachProfileRepository;
            _paymentService = paymentService;
            _jobService = jobService;
        }

        public async Task ExecuteAsync(Guid interviewRoomId)
        {
            var room = await _interviewRoomRepository.GetByIdAsync(interviewRoomId);

            var interviewerId = room.CoachId ?? throw new Exception("InterviewerId is missing for room");
            var coach = await _coachProfileRepository.GetProfileByIdAsync(interviewerId);

            if (room.BookingRequestId == null) return;

            // Find payout transaction via BookingRequest
            InterviewBookingTransaction? t = await _transactionRepository.GetByBookingRequestId(room.BookingRequestId.Value, TransactionType.Payout);

            if (t == null) return;

            if (t.Status == TransactionStatus.Created)
            {
                // Skip payouts with non-positive amount
                if (t.Amount <= 0) return;

                await _paymentService.CreateSpendOrderAsync(
                    t.Amount,
                    $"PAYOUT",
                    coach.BankBinNumber,
                    coach.BankAccountNumber
                );
            }
            // TODO: Refactor payout logic to use in-app balance instead of payout directly to bank account.
            // TODO: Implement retry logic and error handling for payment failures, and consider edge cases such as refunds or disputes that may arise after payout.
            var amount = t.Amount;
            _jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                interviewerId,
                NotificationType.PaymentSuccess,
                "Payout Processed",
                $"Your payout of {amount:N0} resources has been processed.",
                "/dashboard/wallet",
                null
            ));
            
            // TODO: Send email notification to coach about successful payout with details and link to interview history
        }
    }
}
