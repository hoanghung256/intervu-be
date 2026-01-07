using AutoMapper;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Admin;
using Intervu.Domain.Repositories;
using Intervu.Application.Interfaces.UseCases.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Admin
{
    public class GetAllFeedbacks : IGetAllFeedbacks
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IMapper _mapper;

        public GetAllFeedbacks(IFeedbackRepository feedbackRepository, IMapper mapper)
        {
            _feedbackRepository = feedbackRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<FeedbackDto>> ExecuteAsync(int page, int pageSize)
        {
            var (items, total) = await _feedbackRepository.GetPagedFeedbacksAsync(page, pageSize);

            var feedbackDtos = _mapper.Map<List<FeedbackDto>>(items);

            return new PagedResult<FeedbackDto>(feedbackDtos, total, pageSize, page);
        }
    }
}
