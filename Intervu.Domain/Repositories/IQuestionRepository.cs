using Intervu.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Domain.Repositories
{
    public interface IQuestionRepository : IRepositoryBase<Question>
    {
        /// <summary>Returns all questions belonging to a specific InterviewExperience.</summary>
        Task<IEnumerable<Question>> GetByExperienceIdAsync(Guid interviewExperienceId);
    }
}
