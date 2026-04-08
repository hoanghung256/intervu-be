using Intervu.Application.Interfaces.ExternalServices.Email;
using Intervu.Application.Interfaces.UseCases.Email;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Intervu.Application.UseCases.Email
{
    public class SendInterviewReminderEmail : ISendInterviewReminderEmail
    {
        private readonly IInterviewRoomRepository _interviewRoomRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SendInterviewReminderEmail> _logger;

        public SendInterviewReminderEmail(
            IInterviewRoomRepository interviewRoomRepository,
            IUserRepository userRepository,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<SendInterviewReminderEmail> logger)
        {
            _interviewRoomRepository = interviewRoomRepository;
            _userRepository = userRepository;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task ExecuteAsync(Guid roomId, string timeUntil)
        {
            var room = await _interviewRoomRepository.GetByIdWithDetailsAsync(roomId);
            if (room == null)
            {
                return;
            }

            if (room.Status != InterviewRoomStatus.Scheduled ||
                !room.ScheduledTime.HasValue ||
                !room.CandidateId.HasValue ||
                !room.CoachId.HasValue)
            {
                return;
            }

            var candidate = await _userRepository.GetByIdAsync(room.CandidateId.Value);
            var coach = await _userRepository.GetByIdAsync(room.CoachId.Value);

            if (candidate == null && coach == null)
            {
                return;
            }

            var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173";
            var joinLink = room.VideoCallRoomUrl ?? $"{frontendUrl.TrimEnd('/')}/interview/{room.Id}";
            var interviewDate = room.ScheduledTime.Value.ToString("dd MMM yyyy");
            var interviewTime = room.ScheduledTime.Value.ToString("HH:mm");

            if (candidate != null)
            {
                try
                {
                    var candidatePlaceholders = new Dictionary<string, string>
                    {
                        ["RecipientName"] = candidate.FullName,
                        ["OtherPartyName"] = coach?.FullName ?? "Coach",
                        ["InterviewDate"] = interviewDate,
                        ["InterviewTime"] = interviewTime,
                        ["TimeUntil"] = timeUntil,
                        ["JoinLink"] = joinLink
                    };

                    await _emailService.SendEmailWithTemplateAsync(
                        candidate.Email,
                        "InterviewReminder",
                        candidatePlaceholders);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send interview reminder email to candidate for room {RoomId}", roomId);
                }
            }

            if (coach != null)
            {
                try
                {
                    var coachPlaceholders = new Dictionary<string, string>
                    {
                        ["RecipientName"] = coach.FullName,
                        ["OtherPartyName"] = candidate?.FullName ?? "Candidate",
                        ["InterviewDate"] = interviewDate,
                        ["InterviewTime"] = interviewTime,
                        ["TimeUntil"] = timeUntil,
                        ["JoinLink"] = joinLink
                    };

                    await _emailService.SendEmailWithTemplateAsync(
                        coach.Email,
                        "InterviewReminder",
                        coachPlaceholders);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send interview reminder email to coach for room {RoomId}", roomId);
                }
            }
        }
    }
}
