using Intervu.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Domain.Repositories
{
    public interface IFeedbackRepository : IRepositoryBase<Feedback>
    {
        Task<(IReadOnlyList<Feedback> Items, int TotalCount)> GetFeedbacksByStudentIdAsync(int studentId, int page, int pageSize);
        Task<Feedback?> GetFeedbackByIdAsync(int id);
        Task<List<Feedback>> GetFeedbacksByInterviewRoomIdAsync(int interviewRoomId);
        Task CreateFeedbackAsync(Feedback feedback);
        Task UpdateFeedbackAsync(Feedback updatedFeedback);
    }
}
