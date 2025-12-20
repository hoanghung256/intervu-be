using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class InterviewerProfileRepository(IntervuPostgreDbContext context) : RepositoryBase<InterviewerProfile>(context), IInterviewerProfileRepository
    {
        public async Task CreateInterviewerProfile(InterviewerProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile), "Profile cannot be null");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == profile.User.Email);

            if (user == null)
            {
                user = new User
                {
                    FullName = profile.User.FullName,
                    Email = profile.User.Email,
                    Password = profile.User.Password,
                    Role = profile.User.Role,
                    ProfilePicture = profile.User.ProfilePicture,
                    Status = profile.User.Status
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
            }

            profile.User = user;
            await _context.InterviewerProfiles.AddAsync(profile);
            await _context.SaveChangesAsync();
        }

        public async Task<InterviewerProfile> GetProfileAsync()
        {
            throw new NotImplementedException();
        }

        public void DeleteInterviewerProfile(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<InterviewerProfile?> GetProfileByIdAsync(int id)
        {
            InterviewerProfile? profile = await _context.InterviewerProfiles
                .Where(p => p.Id == id)
                .Include(p => p.Companies)
                .Include(p => p.Skills)
                .Include(p => p.User)
                .FirstOrDefaultAsync();

            return profile;
        }

        public async Task<(IReadOnlyList<InterviewerProfile> Items, int TotalCount)> GetPagedInterviewerProfilesAsync(string? search, int? skillId, int? companyId, int page, int pageSize)
        {
            var query = _context.InterviewerProfiles
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

        public async Task UpdateInterviewerProfileAsync(InterviewerProfile updatedProfile)
        {
            var existingProfile = await _context.InterviewerProfiles
                .Include(p => p.Companies)
                .Include(p => p.Skills)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == updatedProfile.Id);

            if (existingProfile == null)
                throw new Exception("Interviewer profile not found.");

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
                existingProfile.User.Email = updatedProfile.User.Email;
                existingProfile.User.ProfilePicture = updatedProfile.User.ProfilePicture;
            }

            await _context.SaveChangesAsync();
        }
    }
}
