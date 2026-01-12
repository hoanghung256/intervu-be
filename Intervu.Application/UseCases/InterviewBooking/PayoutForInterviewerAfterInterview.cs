using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.UseCases.Availability;
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
        private readonly IGetCoachAvailabilities _getCoachAvailabilities;
        private readonly IPaymentService _paymentService;

        public PayoutForCoachAfterInterview(IInterviewRoomRepository interviewRoomRepository, ITransactionRepository transactionRepository, ICoachProfileRepository coachProfileRepository, IGetCoachAvailabilities getCoachAvailabilities, IPaymentService paymentService)
        {
            _interviewRoomRepository = interviewRoomRepository;
            _transactionRepository = transactionRepository;
            _coachProfileRepository = coachProfileRepository;
            _getCoachAvailabilities = getCoachAvailabilities;
            _paymentService = paymentService;
        }

        public async Task ExecuteAsync(Guid interviewRoomId)
        {
            var room = await _interviewRoomRepository.GetByIdAsync(interviewRoomId);

            var interviewerId = room.CoachId ?? throw new Exception("InterviewerId is missing for room");
            var coach = await _coachProfileRepository.GetProfileByIdAsync(interviewerId);

            // Get availability by schedule time + coachId
            CoachAvailability? avai = await _getCoachAvailabilities.GetAsync(interviewerId, (DateTime)room.ScheduledTime);

            if (avai == null) return;
            // Check coach already paid or not
            InterviewBookingTransaction t = await _transactionRepository.GetByAvailabilityId(avai.Id);

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
        }
    }
}
