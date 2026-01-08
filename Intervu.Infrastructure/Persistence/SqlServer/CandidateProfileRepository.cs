using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.SqlServer.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.SqlServer
{
    public class CandidateProfileRepository : RepositoryBase<CandidateProfile>, ICandidateProfileRepository
    {
        public CandidateProfileRepository(IntervuDbContext context) : base(context)
        {
        }

        public async Task CreateCandidateProfileAsync(CandidateProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile), "Profile cannot be null");

            // Expect an already-registered user; do not create accounts here
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == profile.Id);
            if (user == null)
                throw new InvalidOperationException("Registered user not found. Please register the account first.");

            // Ensure EF tracks the existing user and shared PK relation
            profile.User = user;

            await _context.CandidateProfiles.AddAsync(profile);
            await _context.SaveChangesAsync();
        }

        public async Task<CandidateProfile?> GetProfileBySlugAsync(string slug)
        {
            return await _context.CandidateProfiles
                .Where(p => p.User.SlugProfileUrl == slug)
                .Include(p => p.User)
                .Include(p => p.Skills)
                .FirstOrDefaultAsync();
        }

        public async Task<CandidateProfile?> GetProfileByIdAsync(Guid id)
        {
            return await _context.CandidateProfiles
                .Where(p => p.Id == id)
                .Include(p => p.User)
                .Include(p => p.Skills)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateCandidateProfileAsync(CandidateProfile updatedProfile)
        {
            var existingProfile = await _context.CandidateProfiles
                .Include(p => p.User)
                .Include(p => p.Skills)
                .FirstOrDefaultAsync(p => p.Id == updatedProfile.Id);

            if (existingProfile == null)
                throw new Exception("Candidate profile not found.");

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

        public void DeleteCandidateProfile(Guid id)
        {
            var profile = _context.CandidateProfiles.Find(id);
            if (profile != null)
            {
                _context.CandidateProfiles.Remove(profile);
            }
        }
    }
}
