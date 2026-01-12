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
            throw new NotImplementedException();
        }

        public async Task<CoachProfile?> GetProfileBySlugAsync(string slug)
        {
            CoachProfile? profile = await _context.CoachProfiles
                .Where(p => p.User.SlugProfileUrl == slug)
                .Include(p => p.Companies)
                .Include(p => p.Skills)
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
                .Include(p => p.User)
                .FirstOrDefaultAsync();

            return profile;
        }

        public async Task<(IReadOnlyList<CoachProfile> Items, int TotalCount)> GetPagedCoachProfilesAsync(string? search, Guid? skillId, Guid? companyId, int page, int pageSize)
        {
            var query = _context.CoachProfiles
                .Include(i => i.Companies)
                .Include(i => i.Skills)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(i => i.Bio.Contains(search));
            }

            if (skillId.HasValue)
            {
                query = query.Where(x => x.Skills.Any(c => c.Id == skillId.Value));
            }

            if (companyId.HasValue)
            {
                query = query.Where(x => x.Companies.Any(c => c.Id == companyId.Value));
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
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == updatedProfile.Id);

            if (existingProfile == null)
                throw new Exception("Coach profile not found.");

            existingProfile.PortfolioUrl = updatedProfile.PortfolioUrl;
            existingProfile.CurrentAmount = updatedProfile.CurrentAmount;
            existingProfile.ExperienceYears = updatedProfile.ExperienceYears;
            existingProfile.Bio = updatedProfile.Bio;
            existingProfile.BankBinNumber = updatedProfile.BankBinNumber;
            existingProfile.BankAccountNumber = updatedProfile.BankAccountNumber;

            existingProfile.Companies = updatedProfile.Companies ?? new List<Company>();
            existingProfile.Skills = updatedProfile.Skills ?? new List<Skill>();

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
