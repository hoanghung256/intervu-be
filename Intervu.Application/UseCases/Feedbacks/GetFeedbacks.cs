using AutoMapper;
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
        private readonly IMapper _mapper;

        public GetFeedbacks(IFeedbackRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<PagedResult<GetFeedbackResponse>> ExecuteAsync(GetFeedbackRequest request)
        {
            if (request.StudentId.HasValue)
            {
                var (items, total) = await _repo.GetFeedbacksByCandidateIdAsync(request.StudentId.Value, request.Page, request.PageSize);
                var responses = _mapper.Map<IReadOnlyList<GetFeedbackResponse>>(items);
                return new PagedResult<GetFeedbackResponse>(responses.ToList(), total, request.PageSize, request.Page);
            }

            var (pagedItems, pagedTotal) = await _repo.GetPagedFeedbacksAsync(request.Page, request.PageSize);
            var pagedResponses = _mapper.Map<IReadOnlyList<GetFeedbackResponse>>(pagedItems);
            return new PagedResult<GetFeedbackResponse>(pagedResponses.ToList(), pagedTotal, request.PageSize, request.Page);
        }

        public async Task<Feedback?> ExecuteAsync(Guid id)
        {
            return await _repo.GetFeedbackByIdAsync(id);
        }
    }
}
