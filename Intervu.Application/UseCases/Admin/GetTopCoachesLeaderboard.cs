using Intervu.Application.DTOs.Admin;
using Intervu.Application.Interfaces.UseCases.Admin;
using Intervu.Domain.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Admin
{
    public class GetTopCoachesLeaderboard : IGetTopCoachesLeaderboard
    {
        private readonly ICoachProfileRepository _coachProfileRepository;
        private readonly IFeedbackRepository _feedbackRepository;

        public GetTopCoachesLeaderboard(
            ICoachProfileRepository coachProfileRepository,
            IFeedbackRepository feedbackRepository)
        {
            _coachProfileRepository = coachProfileRepository;
            _feedbackRepository = feedbackRepository;
        }

        public async Task<List<CoachPerformanceDto>> ExecuteAsync(int count = 5)
        {
            var coaches = await _coachProfileRepository.GetTopPerformingCoachesAsync(count);
            
            var result = new List<CoachPerformanceDto>();
            int rank = 1;

            foreach (var coach in coaches)
            {
                // In a real system, we'd have these metrics stored or cached. 
                // Using feedback repository to get session count (approximated by feedback count)
                var feedbacks = await _feedbackRepository.GetPagedFeedbacksByFilterAsync(1, 1000, coach.Id);
                var avgRating = await _feedbackRepository.GetAverageRatingByCoachIdAsync(coach.Id);

                result.Add(new CoachPerformanceDto
                {
                    Rank = rank++,
                    Name = coach.User.FullName,
                    Company = coach.Companies.FirstOrDefault()?.Name ?? "Freelance",
                    SessionCount = feedbacks.TotalCount,
                    Rating = Math.Round(avgRating, 1),
                    AvatarUrl = coach.User.ProfilePicture
                });
            }

            return result;
        }
    }
}
