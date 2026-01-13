using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Feedback;
using Intervu.Application.Interfaces.UseCases.Feedbacks;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Feedbacks
{
    public class GetFeedbacks : IGetFeedbacks
    {
        private readonly IFeedbackRepository _repo;

        public GetFeedbacks(IFeedbackRepository repo)
        {
            _repo = repo;
        }

        public async Task<PagedResult<Feedback>> ExecuteAsync(GetFeedbackRequest request)
        {
            if (request.StudentId.HasValue)
            {
                var (items, total) = await _repo.GetFeedbacksByCandidateIdAsync(request.StudentId.Value, request.Page, request.PageSize);
                return new PagedResult<Feedback>(items.ToList(), total, request.PageSize, request.Page);
            }

            var (pagedItems, pagedTotal) = await _repo.GetPagedFeedbacksAsync(request.Page, request.PageSize);
            return new PagedResult<Feedback>(pagedItems.ToList(), pagedTotal, request.PageSize, request.Page);
        }

        public async Task<Feedback?> ExecuteAsync(Guid id)
        {
            return await _repo.GetFeedbackByIdAsync(id);
        }
    }
}
