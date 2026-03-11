using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.InterviewBooking;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.InterviewBooking
{
    public class GetInterviewBookingHistory : IGetInterviewBookingHistory
    {
        private readonly ITransactionRepository _transactionRepository;

        public GetInterviewBookingHistory(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<PagedResult<InterviewBookingTransactionHistoryDto>> ExecuteAsync(Guid userId, GetInterviewBookingHistoryRequest request)
        {
            int page = request.Page <= 0 ? 1 : request.Page;
            int pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 50);

            var (items, total) = await _transactionRepository.GetListByUserAsync(
                userId,
                page,
                pageSize,
                request.Type,
                request.Status);

            var result = items.Select(t => new InterviewBookingTransactionHistoryDto
            {
                Id = t.Id,
                OrderCode = t.OrderCode,
                UserId = t.UserId,
                CoachAvailabilityId = t.CoachAvailabilityId,
                CoachId = t.CoachAvailability?.CoachId,
                StartTime = t.CoachAvailability?.StartTime,
                EndTime = t.CoachAvailability?.EndTime,
                Amount = t.Amount,
                Type = t.Type,
                Status = t.Status
            }).ToList();

            return new PagedResult<InterviewBookingTransactionHistoryDto>(result, total, pageSize, page);
        }
    }
}
