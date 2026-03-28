using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class CoachProfileRepository(IntervuPostgreDbContext context) : RepositoryBase<CoachProfile>(context), ICoachProfileRepository
    {
        public async Task CreateCoachProfileAsync(CoachProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile), "Profile cannot be null");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == profile.User.Email);

            if (user == null)
            {
                user = profile.User;
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
            }
            profile.User.Id = user.Id;
            profile.User = user;

            await _context.CoachProfiles.AddAsync(profile);
            await _context.SaveChangesAsync();
        }

        public async Task<CoachProfile> GetProfileAsync()
        {
            throw new NotImplementedException();
        }

        public void DeleteCoachProfile(Guid id)
        {
            var profile = _context.CoachProfiles.Find(id);
            if (profile != null)
            {
                _context.CoachProfiles.Remove(profile);
            }
        }

        public async Task<CoachProfile?> GetProfileBySlugAsync(string slug)
        {
            CoachProfile? profile = await _context.CoachProfiles
                .Where(p => p.User.SlugProfileUrl == slug)
                .Include(p => p.Companies)
                .Include(p => p.Skills)
                .Include(p => p.Industries)
                .Include(p => p.User)
                .FirstOrDefaultAsync();

            return profile;
        }

        public async Task<CoachProfile?> GetProfileByIdAsync(Guid id)
        {
            CoachProfile? profile = await _context.CoachProfiles
                .Where(p => p.Id == id)
                .Include(p => p.Companies)
                .Include(p => p.Skills)
                .Include(p => p.Industries)
                .Include(p => p.User)
                .FirstOrDefaultAsync();

            return profile;
        }

        public async Task<(IReadOnlyList<CoachProfile> Items, int TotalCount)> GetPagedCoachProfilesAsync(
            string? search,
            Guid? skillId,
            Guid? companyId,
            int page,
            int pageSize,
            List<Guid>? skillIds = null,
            List<string>? levels = null,
            int? minExperienceYears = null,
            int? maxExperienceYears = null,
            int? minPrice = null,
            int? maxPrice = null)
        {
            var query = _context.CoachProfiles
                .Include(i => i.Companies)
                .Include(i => i.Skills)
                .Include(i => i.Industries)
                .Include(i => i.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var normalized = search.Trim().ToLower();
                query = query.Where(i =>
                    (i.Bio ?? string.Empty).ToLower().Contains(normalized)
                    || (i.CurrentJobTitle ?? string.Empty).ToLower().Contains(normalized)
                    || i.Industries.Any(ind => (ind.Name ?? string.Empty).ToLower().Contains(normalized))
                    || (i.User != null && (i.User.FullName ?? string.Empty).ToLower().Contains(normalized)));
            }

            if (skillId.HasValue)
            {
                query = query.Where(x => x.Skills.Any(c => c.Id == skillId.Value));
            }

            if (skillIds != null && skillIds.Count > 0)
            {
                query = query.Where(x => x.Skills.Any(c => skillIds.Contains(c.Id)));
            }

            if (companyId.HasValue)
            {
                query = query.Where(x => x.Companies.Any(c => c.Id == companyId.Value));
            }

            if (minExperienceYears.HasValue)
            {
                query = query.Where(x => (x.ExperienceYears ?? 0) >= minExperienceYears.Value);
            }

            if (maxExperienceYears.HasValue)
            {
                query = query.Where(x => (x.ExperienceYears ?? 0) <= maxExperienceYears.Value);
            }

            if (levels != null && levels.Count > 0)
            {
                var hasJunior = levels.Any(l => string.Equals(l, "Junior", StringComparison.OrdinalIgnoreCase));
                var hasMid = levels.Any(l => string.Equals(l, "Mid", StringComparison.OrdinalIgnoreCase));
                var hasSenior = levels.Any(l => string.Equals(l, "Senior", StringComparison.OrdinalIgnoreCase));

                query = query.Where(x =>
                    (hasJunior && (x.ExperienceYears ?? 0) <= 1)
                    || (hasMid && (x.ExperienceYears ?? 0) >= 2 && (x.ExperienceYears ?? 0) <= 4)
                    || (hasSenior && (x.ExperienceYears ?? 0) >= 5));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(x => (x.CurrentAmount ?? 0) >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(x => (x.CurrentAmount ?? 0) <= maxPrice.Value);
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalItems);
        }

        public async Task UpdateCoachProfileAsync(CoachProfile updatedProfile)
        {
            var existingProfile = await _context.CoachProfiles
                .Include(p => p.Companies)
                .Include(p => p.Skills)
                .Include(p => p.Industries)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == updatedProfile.Id);

            if (existingProfile == null)
                throw new Exception("Coach profile not found.");

            existingProfile.PortfolioUrl = updatedProfile.PortfolioUrl;
            existingProfile.CurrentAmount = updatedProfile.CurrentAmount;
            existingProfile.ExperienceYears = updatedProfile.ExperienceYears;
            existingProfile.CurrentJobTitle = updatedProfile.CurrentJobTitle;
            existingProfile.Bio = updatedProfile.Bio;
            existingProfile.BankBinNumber = updatedProfile.BankBinNumber;
            existingProfile.BankAccountNumber = updatedProfile.BankAccountNumber;

            existingProfile.Companies = updatedProfile.Companies ?? new List<Company>();
            existingProfile.Skills = updatedProfile.Skills ?? new List<Skill>();
            existingProfile.Industries = updatedProfile.Industries ?? new List<Industry>();

            if (existingProfile.User != null && updatedProfile.User != null)
            {
                existingProfile.User.FullName = updatedProfile.User.FullName;
                existingProfile.User.SlugProfileUrl = updatedProfile.User.SlugProfileUrl;
                existingProfile.User.Email = updatedProfile.User.Email;
                existingProfile.User.ProfilePicture = updatedProfile.User.ProfilePicture;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalCoachCountAsync()
        {
            return await _context.CoachProfiles.CountAsync();
        }
    }
}
