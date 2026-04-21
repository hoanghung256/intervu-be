using Intervu.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Domain.Repositories
{
    public interface IPreparedQuestionRepository : IRepositoryBase<PreparedQuestion>
    {
        Task<List<PreparedQuestion>> GetByInterviewRoomIdAsync(Guid interviewRoomId);

        Task<int> GetMaxSortOrderAsync(Guid interviewRoomId);

        Task<PreparedQuestion?> FindByRoomAndBankQuestionAsync(Guid interviewRoomId, Guid bankQuestionId);

        Task<List<PreparedQuestion>> GetByIdsAsync(Guid interviewRoomId, IEnumerable<Guid> ids);
    }
}
