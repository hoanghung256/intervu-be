using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.Feedbacks;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Feedbacks
{
    public class UpdateFeedback : IUpdateFeedback
    {
        private readonly IFeedbackRepository _repo;
        private readonly IBackgroundService _backgroundService;
        private readonly IInterviewRoomRepository _interviewRoomRepository;

        public UpdateFeedback(
            IFeedbackRepository repo,
            IBackgroundService backgroundService,
            IInterviewRoomRepository interviewRoomRepository)
        {
            _repo = repo;
            _backgroundService = backgroundService;
            _interviewRoomRepository = interviewRoomRepository;
        }

        public async Task ExecuteAsync(Feedback updatedFeedback)
        {
            await _repo.UpdateFeedbackAsync(updatedFeedback);

            var bookingRequestId = (await _interviewRoomRepository.GetByIdAsync(updatedFeedback.InterviewRoomId))
                ?.BookingRequestId;
            var actionUrl = bookingRequestId.HasValue
                ? $"/booking-requests/{bookingRequestId}?action=feedback"
                : "/booking-requests";

            // Notify coach about new feedback
            _backgroundService.Enqueue<INotificationUseCase>(
                uc => uc.CreateAsync(
                    updatedFeedback.CoachId,
                    NotificationType.FeedbackReceived,
                    "New feedback received",
                    "A candidate has left feedback for your interview session.",
                    actionUrl,
                    updatedFeedback.Id));
        }
    }
}
