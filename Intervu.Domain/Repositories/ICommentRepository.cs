using Intervu.Domain.Entities;

namespace Intervu.Domain.Repositories
{
    public interface ICommentRepository : IRepositoryBase<Comment>
    {
        Task<List<Comment>> GetByQuestionIdAsync(Guid questionId);
    }
}
