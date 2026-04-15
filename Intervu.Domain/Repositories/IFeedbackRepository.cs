using Intervu.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Domain.Repositories
{
    public interface IFeedbackRepository : IRepositoryBase<Feedback>
    {
        Task<(IReadOnlyList<Feedback> Items, int TotalCount)> GetPagedFeedbacksAsync(int page, int pageSize);
        Task<(IReadOnlyList<Feedback> Items, int TotalCount)> GetPagedFeedbacksByFilterAsync(int page, int pageSize, Guid? coachId = null);
        Task<int> GetTotalFeedbacksCountAsync();
        Task<double> GetAverageRatingAsync();
        Task<(double AverageRating, int TotalRatings)> GetAverageRatingByCoachIdAsync(Guid coachId);
        Task<(double AverageRating, int TotalRatings)> GetAverageRatingByCandidateIdAsync(Guid candidateId);
        Task<(IReadOnlyList<Feedback> Items, int TotalCount)> GetFeedbacksByCandidateIdAsync(Guid candidateId, int page, int pageSize);
        Task<Feedback?> GetFeedbackByIdAsync(Guid id);
        Task<List<Feedback>> GetFeedbacksByInterviewRoomIdAsync(Guid interviewRoomId);
        Task<Dictionary<Guid, double?>> GetRatingsByInterviewRoomIdsAsync(IEnumerable<Guid> interviewRoomIds);
        Task CreateFeedbackAsync(Feedback feedback);
        Task UpdateFeedbackAsync(Feedback updatedFeedback);

        /// <summary>
        /// Get recent feedbacks for a coach with candidate name, ordered by interview scheduled time descending.
        /// </summary>
        Task<List<(Feedback Feedback, string CandidateName)>> GetRecentFeedbacksByCoachIdAsync(Guid coachId, int limit);
    }
}
