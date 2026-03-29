using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Domain.Repositories
{
    public interface IQuestionReportRepository : IRepositoryBase<QuestionReport>
    {
        Task<bool> HasPendingReportAsync(Guid questionId, Guid userId);
        Task<(List<QuestionReport> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, QuestionReportStatus? status = null, string? search = null);
    }
}