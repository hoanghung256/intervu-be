using AutoMapper;
using Intervu.Application.DTOs.BookingRequest;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.BookingRequest;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.BookingRequest
{
    internal class CancelBookingRequest : ICancelBookingRequest
    {
        private readonly IBookingRequestRepository _bookingRepo;
        private readonly IMapper _mapper;

        public CancelBookingRequest(IBookingRequestRepository bookingRepo, IMapper mapper)
        {
            _bookingRepo = bookingRepo;
            _mapper = mapper;
        }

        public async Task<BookingRequestDto> ExecuteAsync(Guid candidateId, Guid bookingRequestId)
        {
            var bookingRequest = await _bookingRepo.GetByIdWithDetailsAsync(bookingRequestId)
                ?? throw new NotFoundException("Booking request not found");

            // Only the owning candidate can cancel
            if (bookingRequest.CandidateId != candidateId)
                throw new ForbiddenException("You can only cancel your own booking requests");

            // Only Pending or Accepted requests can be cancelled
            if (bookingRequest.Status != BookingRequestStatus.Pending &&
                bookingRequest.Status != BookingRequestStatus.Accepted)
            {
                throw new BadRequestException(
                    $"Cannot cancel a booking request with status '{bookingRequest.Status}'. " +
                    "Only Pending or Accepted requests can be cancelled.");
            }

            bookingRequest.Status = BookingRequestStatus.Cancelled;
            bookingRequest.UpdatedAt = DateTime.UtcNow;

            _bookingRepo.UpdateAsync(bookingRequest);
            await _bookingRepo.SaveChangesAsync();

            var result = _mapper.Map<BookingRequestDto>(bookingRequest);
            result.CandidateName = bookingRequest.Candidate?.User?.FullName;
            result.CoachName = bookingRequest.Coach?.User?.FullName;

            return result;
        }
    }
}
