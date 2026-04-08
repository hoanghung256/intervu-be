using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.ExternalServices.Email;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Application.UseCases.Availability;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Configuration;

namespace Intervu.Application.UseCases.InterviewBooking
{
    public class PayoutForCoachAfterInterview : IPayoutForCoachAfterInterview
    {
        private readonly IInterviewRoomRepository _interviewRoomRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICoachProfileRepository _coachProfileRepository;
        private readonly IGetCoachAvailabilities _getCoachAvailabilities;
        private readonly IPaymentService _paymentService;
        private readonly IBackgroundService _jobService;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public PayoutForCoachAfterInterview(
            IInterviewRoomRepository interviewRoomRepository, 
            ITransactionRepository transactionRepository, 
            ICoachProfileRepository coachProfileRepository, 
            IGetCoachAvailabilities getCoachAvailabilities, 
            IPaymentService paymentService,
            IBackgroundService jobService,
            IUserRepository userRepository,
            IConfiguration configuration)
        {
            _interviewRoomRepository = interviewRoomRepository;
            _transactionRepository = transactionRepository;
            _coachProfileRepository = coachProfileRepository;
            _getCoachAvailabilities = getCoachAvailabilities;
            _paymentService = paymentService;
            _jobService = jobService;
            _userRepository = userRepository;
            _configuration = configuration;
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

            var coachUser = await _userRepository.GetByIdAsync(interviewerId);
            if (coachUser != null)
            {
                var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173";
                var placeholders = new Dictionary<string, string>
                {
                    ["CoachName"] = coachUser.FullName,
                    ["Amount"] = amount.ToString("N0"),
                    ["DashboardLink"] = $"{frontendUrl.TrimEnd('/')}/dashboard/wallet"
                };

                _jobService.Enqueue<IEmailService>(svc => svc.SendEmailWithTemplateAsync(
                    coachUser.Email,
                    "PayoutConfirmation",
                    placeholders));
            }
        }
    }
}
