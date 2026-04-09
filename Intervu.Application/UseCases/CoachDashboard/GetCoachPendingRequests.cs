using Intervu.Application.DTOs.CoachDashboard;
using Intervu.Application.Interfaces.UseCases.CoachDashboard;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.CoachDashboard
{
    public class GetCoachPendingRequests : IGetCoachPendingRequests
    {
        private readonly IBookingRequestRepository _bookingRepo;

        public GetCoachPendingRequests(IBookingRequestRepository bookingRepo)
        {
            _bookingRepo = bookingRepo;
        }

        public async Task<List<CoachPendingRequestDto>> ExecuteAsync(Guid coachId)
        {
            var (items, _) = await _bookingRepo.GetPagedByCoachIdAsync(
                coachId, null, new List<BookingRequestStatus> { BookingRequestStatus.Pending }, 1, 20);

            return items.Select(br =>
            {
                var candidateUser = br.Candidate?.User;

                return new CoachPendingRequestDto
                {
                    BookingRequestId = br.Id,
                    CandidateName = candidateUser?.FullName ?? "Unknown",
                    CandidateProfilePicture = candidateUser?.ProfilePicture,
                    CandidateJobTitle = br.Candidate?.Bio,
                    CandidateExperienceYears = null,
                    Message = br.JobDescriptionUrl != null
                        ? "JD-based interview request"
                        : null,
                    RequestedAt = br.CreatedAt
                };
            }).ToList();
        }
    }
}
