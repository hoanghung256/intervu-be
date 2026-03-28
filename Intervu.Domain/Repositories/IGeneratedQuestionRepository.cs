using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Domain.Repositories
{
    public interface IGeneratedQuestionRepository : IRepositoryBase<GeneratedQuestion>
    {
        Task<List<GeneratedQuestion>> GetByInterviewRoomIdAsync(Guid interviewRoomId);

        Task<List<GeneratedQuestion>> GetByInterviewRoomIdAsync(Guid interviewRoomId, GeneratedQuestionStatus status);
    }
}
