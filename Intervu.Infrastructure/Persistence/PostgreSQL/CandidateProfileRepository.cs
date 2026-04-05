using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class CandidateProfileRepository : RepositoryBase<CandidateProfile>, ICandidateProfileRepository
    {
        public CandidateProfileRepository(IntervuPostgreDbContext context) : base(context)
        {
        }

        public async Task CreateCandidateProfileAsync(CandidateProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile), "Profile cannot be null");

            var user = await _context.CandidateProfiles.FirstOrDefaultAsync(u => u.Id == profile.Id);
            if (user == null)
                throw new InvalidOperationException("Registered user not found. Please register the account first.");

            await _context.SaveChangesAsync();
        }

        public async Task<CandidateProfile?> GetProfileBySlugAsync(string slug)
        {
            return await _context.CandidateProfiles
                .Where(p => p.User.SlugProfileUrl == slug)
                .Include(p => p.User)
                .Include(p => p.Skills)
                .Include(p => p.Industries)
                .Include(p => p.WorkExperiences)
                .FirstOrDefaultAsync();
        }

        public async Task<CandidateProfile?> GetProfileByIdAsync(Guid id)
        {
            return await _context.CandidateProfiles
                .Where(p => p.Id == id)
                .Include(p => p.User)
                .Include(p => p.Skills)
                .Include(p => p.Industries)
                .Include(p => p.WorkExperiences)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateCandidateProfileAsync(CandidateProfile updatedProfile)
        {
            var existingProfile = await _context.CandidateProfiles
                .Include(p => p.User)
                .Include(p => p.Skills)
                .Include(p => p.Industries)
                .Include(p => p.WorkExperiences)
                .FirstOrDefaultAsync(p => p.Id == updatedProfile.Id);

            if (existingProfile == null)
                throw new Exception("Candidate profile not found.");

            if (updatedProfile.CVUrl != null) existingProfile.CVUrl = updatedProfile.CVUrl;
            if (updatedProfile.PortfolioUrl != null) existingProfile.PortfolioUrl = updatedProfile.PortfolioUrl;
            if (updatedProfile.Bio != null) existingProfile.Bio = updatedProfile.Bio;
            existingProfile.CurrentAmount = updatedProfile.CurrentAmount;

            if (updatedProfile.Skills != null)
            {
                existingProfile.Skills.Clear();
                var skillIds = updatedProfile.Skills.Select(s => s.Id).ToList();
                var skills = await _context.Skills.Where(s => skillIds.Contains(s.Id)).ToListAsync();
                foreach (var skill in skills)
                {
                    existingProfile.Skills.Add(skill);
                }
            }

            if (updatedProfile.Industries != null)
            {
                existingProfile.Industries.Clear();
                var industryIds = updatedProfile.Industries.Select(i => i.Id).ToList();
                var industries = await _context.Industries.Where(i => industryIds.Contains(i.Id)).ToListAsync();
                foreach (var industry in industries)
                {
                    existingProfile.Industries.Add(industry);
                }
            }

            if (updatedProfile.CertificationLinks != null)
            {
                existingProfile.CertificationLinks = updatedProfile.CertificationLinks;
            }

            if (existingProfile.User != null && updatedProfile.User != null)
            {
                if (!string.IsNullOrWhiteSpace(updatedProfile.User.FullName))
                    existingProfile.User.FullName = updatedProfile.User.FullName;
                if (!string.IsNullOrWhiteSpace(updatedProfile.User.Email))
                    existingProfile.User.Email = updatedProfile.User.Email;
                if (!string.IsNullOrWhiteSpace(updatedProfile.User.ProfilePicture))
                    existingProfile.User.ProfilePicture = updatedProfile.User.ProfilePicture;
            }

            await _context.SaveChangesAsync();
        }

        public async Task ReplaceWorkExperiencesAsync(Guid candidateId, IEnumerable<CandidateWorkExperience> workExperiences)
        {
            var existingProfile = await _context.CandidateProfiles
                .Include(p => p.WorkExperiences)
                .FirstOrDefaultAsync(p => p.Id == candidateId);

            if (existingProfile == null)
                throw new Exception("Candidate profile not found.");

            _context.Set<CandidateWorkExperience>().RemoveRange(existingProfile.WorkExperiences);

            var items = workExperiences?.ToList() ?? new List<CandidateWorkExperience>();
            foreach (var item in items)
            {
                item.CandidateProfileId = candidateId;
            }

            await _context.Set<CandidateWorkExperience>().AddRangeAsync(items);
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
