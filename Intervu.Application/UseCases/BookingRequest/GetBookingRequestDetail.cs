using AutoMapper;
using Intervu.Application.DTOs.BookingRequest;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.BookingRequest;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.BookingRequest
{
    internal class GetBookingRequestDetail : IGetBookingRequestDetail
    {
        private readonly IBookingRequestRepository _bookingRepo;
        private readonly IMapper _mapper;

        public GetBookingRequestDetail(IBookingRequestRepository bookingRepo, IMapper mapper)
        {
            _bookingRepo = bookingRepo;
            _mapper = mapper;
        }

        public async Task<BookingRequestDto> ExecuteAsync(Guid userId, Guid bookingRequestId)
        {
            var bookingRequest = await _bookingRepo.GetByIdWithDetailsAsync(bookingRequestId)
                ?? throw new NotFoundException("Booking request not found");

            // Only the candidate or coach involved can view the detail
            if (bookingRequest.CandidateId != userId && bookingRequest.CoachId != userId)
                throw new ForbiddenException("You do not have access to this booking request");

            var result = _mapper.Map<BookingRequestDto>(bookingRequest);
            result.CandidateName = bookingRequest.Candidate?.User?.FullName;
            result.CoachName = bookingRequest.Coach?.User?.FullName;

            return result;
        }
    }
}
