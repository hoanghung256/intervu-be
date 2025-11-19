using Intervu.Application.Common;
using Intervu.Application.DTOs.Feedback;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.Feedbacks;
using Intervu.Domain.Entities;
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
            return await _repo.GetFeedbacksByStudentIdAsync(request);
        }

        public async Task<Feedback?> ExecuteAsync(int id)
        {
            return await _repo.GetFeedbackByIdAsync(id);
        }
    }
}
