using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.UseCases.Availability;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.UseCases.InterviewBooking
{
    public class PayoutForInterviewerAfterInterview : IPayoutForInterviewerAfterInterview
    {
        private readonly IInterviewRoomRepository _interviewRoomRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IInterviewerProfileRepository _interviewerProfileRepository;
        private readonly IGetInterviewerAvailabilities _getInterviewerAvailabilities;
        private readonly IPaymentService _paymentService;

        public PayoutForInterviewerAfterInterview(IInterviewRoomRepository interviewRoomRepository, ITransactionRepository transactionRepository, IInterviewerProfileRepository interviewerProfileRepository, IGetInterviewerAvailabilities getInterviewerAvailabilities, IPaymentService paymentService)
        {
            _interviewRoomRepository = interviewRoomRepository;
            _transactionRepository = transactionRepository;
            _interviewerProfileRepository = interviewerProfileRepository;
            _getInterviewerAvailabilities = getInterviewerAvailabilities;
            _paymentService = paymentService;
        }

        public async Task ExecuteAsync(int interviewRoomId)
        {
            var room = await _interviewRoomRepository.GetByIdAsync(interviewRoomId);

            int interviewerId = (int)room.InterviewerId;
            var interviewer = await _interviewerProfileRepository.GetProfileByIdAsync(interviewerId);

            // Get availability by schedule time + interviewerId
            InterviewerAvailability avai = await _getInterviewerAvailabilities.GetAsync(interviewerId, (DateTime)room.ScheduledTime);

            if (avai == null) return;
            // Check interviewer aleary paid or not
            InterviewBookingTransaction t = await _transactionRepository.GetByAvailabilityId(avai.Id);

            if (t.Status == TransactionStatus.Created)
            {
                await _paymentService.CreateSpendOrderAsync(
                    t.Amount,
                    $"PAYOUT",
                    interviewer.BankBinNumber,
                    interviewer.BankAccountNumber
                );
            }
        }
    }
}
