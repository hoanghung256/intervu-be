using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Domain.Entities;
using Intervu.Infrastructure.Persistence.SqlServer.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.SqlServer
{
    public class InterviewerAvailabilitiesRepository : IInterviewerAvailabilitiesRepository
    {
        private readonly IntervuDbContext _dbContext;
        public InterviewerAvailabilitiesRepository(IntervuDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<InterviewerAvailability>> GetInterviewerAvailabilitiesByMonthAsync(int intervewerId, int month = 0, int year = 0)
        {
            var query = _dbContext.InterviewerAvailabilities.AsQueryable();

            var filtered = query.Where(x => x.InterviewerId == intervewerId);

            if (month > 0 && year > 0)
            {
                filtered = filtered.Where(x => x.StartTime.Month == month && x.StartTime.Year == year);
            }
            else if (month > 0)
            {
                filtered = filtered.Where(x => x.StartTime.Month == month);
            }
            else if (year > 0)
            {
                filtered = filtered.Where(x => x.StartTime.Year == year);
            }

            var result = await filtered.ToListAsync();
            return result;
        }

        public async Task<bool> IsInterviewerAvailableAsync(int interviewerId, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            // return true if no overlapping availability or booking exists
            var overlaps = await _dbContext.InterviewerAvailabilities
                .Where(x => x.InterviewerId == interviewerId)
                .Where(x => !(x.EndTime <= startTime.UtcDateTime || x.StartTime >= endTime.UtcDateTime))
                .AnyAsync();
            return !overlaps;
        }

        public async Task<int> CreateInterviewerAvailabilityAsync(InterviewerAvailability availability)
        {
            _dbContext.InterviewerAvailabilities.Add(availability);
            await _dbContext.SaveChangesAsync();
            return availability.Id;
        }

        public async Task<bool> DeleteInterviewerAvailabilityAsync(int availabilityId)
        {
            var availability = await _dbContext.InterviewerAvailabilities.FindAsync(availabilityId);
            if (availability == null)
                return false;

            _dbContext.InterviewerAvailabilities.Remove(availability);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateInterviewerAvailabilityAsync(int availabilityId, InterviewerAvailabilityUpdateDto dto)
        {
            var availability = await _dbContext.InterviewerAvailabilities.FindAsync(availabilityId);
            if (availability == null)
                return false;

            availability.StartTime = dto.StartTime;
            availability.EndTime = dto.EndTime;

            _dbContext.InterviewerAvailabilities.Update(availability);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
