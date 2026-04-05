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
        private readonly IInterviewRoomRepository _roomRepo;
        private readonly IMapper _mapper;

        public GetBookingRequestDetail(
            IBookingRequestRepository bookingRepo, 
            IInterviewRoomRepository roomRepo,
            IMapper mapper)
        {
            _bookingRepo = bookingRepo;
            _roomRepo = roomRepo;
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

            // --- Cleanup IC-58 & Multiple Rounds Fix ---
            if (result.InterviewRoomId == null || (result.Rounds != null && result.Rounds.Any(r => r.InterviewRoomId == null)))
            {
                var rooms = await _roomRepo.GetByBookingRequestIdAsync(bookingRequest.Id);
                
                // Flow B (Single session / External)
                if (result.Rounds == null || result.Rounds.Count == 0)
                {
                    var firstRoom = rooms.FirstOrDefault();
                    if (firstRoom != null)
                    {
                        result.InterviewRoomId = firstRoom.Id;
                        result.InterviewRoomStatus = firstRoom.Status.ToString();
                    }
                }
                // Flow C (Multi-round / JD Interview)
                else
                {
                    foreach (var round in result.Rounds)
                    {
                        if (round.InterviewRoomId == null)
                        {
                            var matchingRoom = rooms.FirstOrDefault(r => r.RoundNumber == round.RoundNumber);
                            if (matchingRoom != null)
                            {
                                round.InterviewRoomId = matchingRoom.Id;
                                round.InterviewRoomStatus = matchingRoom.Status.ToString();
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
