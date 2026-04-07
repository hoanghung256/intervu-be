using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class InterviewRoomRepository(IntervuPostgreDbContext context) : RepositoryBase<InterviewRoom>(context), IInterviewRoomRepository
    {
        public async Task<IEnumerable<InterviewRoom>> GetListByCandidateId(Guid candidateId)
        {
            return await _context.InterviewRooms.Where(r => r.CandidateId == candidateId).ToListAsync();
        }

        public async Task<IEnumerable<InterviewRoom>> GetListByCoachId(Guid coachId)
        {
            return await _context.InterviewRooms.Where(r => r.CoachId == coachId).ToListAsync();
        }

        public async Task<IEnumerable<InterviewRoom>> GetList()
        {
            return await _context.InterviewRooms.ToListAsync();
        }

        public async Task<IEnumerable<(
            InterviewRoom Room,
            string? CandidateName,
            string? CandidateProfilePicture,
            string? CandidateSlugProfileUrl,
            string? CoachName,
            string? CoachProfilePicture,
            string? CoachSlugProfileUrl)>> GetListWithNamesByCandidateIdAsync(Guid candidateId)
        {
            var rooms = await _context.InterviewRooms
                .Include(r => r.CurrentAvailability)
                .Include(r => r.CoachInterviewService)!
                    .ThenInclude(s => s!.InterviewType)
                .Where(r => r.CandidateId == candidateId)
                .ToListAsync();

            return await BuildRoomTuplesWithParticipantSummaryAsync(rooms);
        }

        public async Task<IEnumerable<(
            InterviewRoom Room,
            string? CandidateName,
            string? CandidateProfilePicture,
            string? CandidateSlugProfileUrl,
            string? CoachName,
            string? CoachProfilePicture,
            string? CoachSlugProfileUrl)>> GetListWithNamesByCoachIdAsync(Guid coachId)
        {
            var rooms = await _context.InterviewRooms
                .Include(r => r.CurrentAvailability)
                .Include(r => r.CoachInterviewService)!
                    .ThenInclude(s => s!.InterviewType)
                .Where(r => r.CoachId == coachId)
                .ToListAsync();

            return await BuildRoomTuplesWithParticipantSummaryAsync(rooms);
        }

        private async Task<IEnumerable<(
            InterviewRoom Room,
            string? CandidateName,
            string? CandidateProfilePicture,
            string? CandidateSlugProfileUrl,
            string? CoachName,
            string? CoachProfilePicture,
            string? CoachSlugProfileUrl)>> BuildRoomTuplesWithParticipantSummaryAsync(List<InterviewRoom> rooms)
        {
            var userIds = rooms
                .SelectMany(r => new Guid?[] { r.CandidateId, r.CoachId })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var usersById = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.ProfilePicture,
                    u.SlugProfileUrl,
                })
                .ToDictionaryAsync(u => u.Id);

            var result = new List<(
                InterviewRoom Room,
                string? CandidateName,
                string? CandidateProfilePicture,
                string? CandidateSlugProfileUrl,
                string? CoachName,
                string? CoachProfilePicture,
                string? CoachSlugProfileUrl)>();

            foreach (var room in rooms)
            {
                usersById.TryGetValue(room.CandidateId ?? Guid.Empty, out var candidateUser);
                usersById.TryGetValue(room.CoachId ?? Guid.Empty, out var coachUser);

                result.Add((
                    room,
                    candidateUser?.FullName,
                    candidateUser?.ProfilePicture,
                    candidateUser?.SlugProfileUrl,
                    coachUser?.FullName,
                    coachUser?.ProfilePicture,
                    coachUser?.SlugProfileUrl));
            }

            return result;
        }

        public async Task<IEnumerable<InterviewRoom>> GetConflictingRoomsAsync(Guid userId, DateTime startTime, DateTime endTime)
        {
            return await _context.InterviewRooms
                .Where(r => 
                    (r.CandidateId == userId || r.CoachId == userId) &&
                    r.ScheduledTime != null &&
                    r.Status != InterviewRoomStatus.Cancelled &&
                    r.Status != InterviewRoomStatus.Completed &&
                    // Check overlap: (StartA < EndB) and (EndA > StartB)
                    r.ScheduledTime < endTime &&
                    r.ScheduledTime.Value.AddMinutes(r.DurationMinutes ?? 60) > startTime)
                .ToListAsync();
        }

        public async Task<InterviewRoom?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.InterviewRooms
                .Include(r => r.Transaction)
                .Include(r => r.CurrentAvailability)
                .Include(r => r.CoachInterviewService)!
                    .ThenInclude(s => s!.InterviewType)
                .Include(r => r.RescheduleRequests)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<InterviewRoom>> GetByBookingRequestIdAsync(Guid bookingRequestId)
        {
            return await _context.InterviewRooms
                .Where(r => r.BookingRequestId == bookingRequestId)
                .ToListAsync();
        }
    }
}
