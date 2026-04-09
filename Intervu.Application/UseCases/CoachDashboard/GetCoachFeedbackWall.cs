using Intervu.Application.DTOs.CoachDashboard;
using Intervu.Application.Interfaces.UseCases.CoachDashboard;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.CoachDashboard
{
    public class GetCoachFeedbackWall : IGetCoachFeedbackWall
    {
        private readonly IFeedbackRepository _feedbackRepo;

        public GetCoachFeedbackWall(IFeedbackRepository feedbackRepo)
        {
            _feedbackRepo = feedbackRepo;
        }

        public async Task<List<CoachFeedbackItemDto>> ExecuteAsync(Guid coachId)
        {
            var feedbacks = await _feedbackRepo.GetRecentFeedbacksByCoachIdAsync(coachId, 5);

            return feedbacks.Select(f => new CoachFeedbackItemDto
            {
                Rating = f.Feedback.Rating,
                Comments = f.Feedback.Comments ?? string.Empty,
                CandidateName = f.CandidateName,
                CreatedAt = f.Feedback.InterviewRoom?.ScheduledTime ?? DateTime.UtcNow
            }).ToList();
        }
    }
}
