using AutoMapper;
using Intervu.Application.DTOs.BookingRequest;
using Intervu.Application.Interfaces.UseCases.BookingRequest;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.BookingRequest
{
    internal class GetBookingRequests : IGetBookingRequests
    {
        private readonly IBookingRequestRepository _bookingRepo;
        private readonly IMapper _mapper;

        public GetBookingRequests(IBookingRequestRepository bookingRepo, IMapper mapper)
        {
            _bookingRepo = bookingRepo;
            _mapper = mapper;
        }

        public async Task<(IReadOnlyList<BookingRequestDto> Items, int TotalCount)> ExecuteAsync(
            Guid userId, bool isCoach, GetBookingRequestsFilterDto filter)
        {
            var (items, totalCount) = isCoach
                ? await _bookingRepo.GetPagedByCoachIdAsync(
                    userId, filter.Type, filter.Statuses, filter.Page, filter.PageSize)
                : await _bookingRepo.GetPagedByCandidateIdAsync(
                    userId, filter.Type, filter.Statuses, filter.Page, filter.PageSize);

            var dtos = items.Select(br =>
            {
                var dto = _mapper.Map<BookingRequestDto>(br);
                dto.CandidateName = br.Candidate?.User?.FullName;
                dto.CoachName = br.Coach?.User?.FullName;
                return dto;
            }).ToList().AsReadOnly();

            return (dtos, totalCount);
        }
    }
}
