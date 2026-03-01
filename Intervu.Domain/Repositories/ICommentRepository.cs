using Intervu.Domain.Entities;

namespace Intervu.Domain.Repositories
{
    public interface ICommentRepository : IRepositoryBase<Comment>
    {
        Task<List<Comment>> GetByQuestionIdAsync(Guid questionId);
        Task<(List<Comment> Items, int TotalCount)> GetPagedByQuestionIdAsync(Guid questionId, int page, int pageSize);
    }
}
