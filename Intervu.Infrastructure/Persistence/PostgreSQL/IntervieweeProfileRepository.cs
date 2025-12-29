using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class IntervieweeProfileRepository : RepositoryBase<IntervieweeProfile>, IIntervieweeProfileRepository
    {
        public IntervieweeProfileRepository(IntervuPostgreDbContext context) : base(context)
        {
        }

        public async Task CreateIntervieweeProfileAsync(IntervieweeProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile), "Profile cannot be null");

            var user = await _context.IntervieweeProfiles.FirstOrDefaultAsync(u => u.Id == profile.Id);
            if (user == null)
                throw new InvalidOperationException("Registered user not found. Please register the account first.");

            //profile.User = user;

            await _context.SaveChangesAsync();
        }

        public async Task<IntervieweeProfile?> GetProfileBySlugAsync(string slug)
        {
            return await _context.IntervieweeProfiles
                .Where(p => p.User.SlugProfileUrl == slug)
                .Include(p => p.User)
                .Include(p => p.Skills)
                .FirstOrDefaultAsync();
        }

        public async Task<IntervieweeProfile?> GetProfileByIdAsync(Guid id)
        {
            return await _context.IntervieweeProfiles
                .Where(p => p.Id == id)
                .Include(p => p.User)
                .Include(p => p.Skills)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateIntervieweeProfileAsync(IntervieweeProfile updatedProfile)
        {
            var existingProfile = await _context.IntervieweeProfiles
                .Include(p => p.User)
                .Include(p => p.Skills)
                .FirstOrDefaultAsync(p => p.Id == updatedProfile.Id);

            if (existingProfile == null)
                throw new Exception("Interviewee profile not found.");

            existingProfile.CVUrl = updatedProfile.CVUrl;
            existingProfile.PortfolioUrl = updatedProfile.PortfolioUrl;
            existingProfile.Skills = updatedProfile.Skills;
            existingProfile.Bio = updatedProfile.Bio;
            existingProfile.CurrentAmount = updatedProfile.CurrentAmount;

            if (existingProfile.User != null && updatedProfile.User != null)
            {
                existingProfile.User.FullName = updatedProfile.User.FullName;
                existingProfile.User.Email = updatedProfile.User.Email;
                existingProfile.User.ProfilePicture = updatedProfile.User.ProfilePicture;
            }

            await _context.SaveChangesAsync();
        }

        public void DeleteIntervieweeProfile(Guid id)
        {
            var profile = _context.IntervieweeProfiles.Find(id);
            if (profile != null)
            {
                _context.IntervieweeProfiles.Remove(profile);
            }
        }
    }
}
