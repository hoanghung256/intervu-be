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
            profile.Id = user.Id;

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

        public async Task<CoachProfile?> GetProfileBySlugAsync(String slug)
        {
            CoachProfile? profile = await _context.CoachProfiles
                .Where(p => p.User.SlugProfileUrl == slug)
                .Include(p => p.Companies)
                .Include(p => p.Skills)
                .Include(p => p.Industries)
                .Include(p => p.WorkExperiences)
                .Include(p => p.Certificates)
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
                .Include(p => p.Certificates)
                .Include(p => p.WorkExperiences)
                .Include(p => p.User)
                .FirstOrDefaultAsync();

            return profile;
        }

        public async Task<(IReadOnlyList<CoachProfile> Items, int TotalCount)> GetPagedCoachProfilesAsync(
            string? search,
            Guid? skillId,
            Guid? companyId,
            Guid? industryId,
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
                .Include(i => i.Certificates)
                .Include(i => i.WorkExperiences)
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

            if (industryId.HasValue)
            {
                query = query.Where(x => x.Industries.Any(i => i.Id == industryId.Value));
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
                .Include(p => p.WorkExperiences)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == updatedProfile.Id);

            if (existingProfile == null)
                throw new Exception("Coach profile not found.");

            if (updatedProfile.PortfolioUrl != null) existingProfile.PortfolioUrl = updatedProfile.PortfolioUrl;
            if (updatedProfile.CurrentAmount != null) existingProfile.CurrentAmount = updatedProfile.CurrentAmount;
            if (updatedProfile.ExperienceYears != null) existingProfile.ExperienceYears = updatedProfile.ExperienceYears;
            if (updatedProfile.CurrentJobTitle != null) existingProfile.CurrentJobTitle = updatedProfile.CurrentJobTitle;
            if (updatedProfile.Bio != null) existingProfile.Bio = updatedProfile.Bio;
            if (updatedProfile.BankBinNumber != null) existingProfile.BankBinNumber = updatedProfile.BankBinNumber;
            if (updatedProfile.BankAccountNumber != null) existingProfile.BankAccountNumber = updatedProfile.BankAccountNumber;

            if (updatedProfile.Companies != null)
            {
                var companyIds = updatedProfile.Companies.Select(c => c.Id).ToList();
                var companies = await _context.Companies.Where(c => companyIds.Contains(c.Id)).ToListAsync();
                foreach (var company in companies)
                {
                    existingProfile.Companies.Add(company);
                }
            }

            if (updatedProfile.Skills != null)
            {
                var skillIds = updatedProfile.Skills.Select(s => s.Id).ToList();
                var skills = await _context.Skills.Where(s => skillIds.Contains(s.Id)).ToListAsync();
                foreach (var skill in skills)
                {
                    existingProfile.Skills.Add(skill);
                }
            }

            if (updatedProfile.Industries != null)
            {
                var industryIds = updatedProfile.Industries.Select(i => i.Id).ToList();
                var industries = await _context.Industries.Where(i => industryIds.Contains(i.Id)).ToListAsync();
                foreach (var industry in industries)
                {
                    existingProfile.Industries.Add(industry);
                }
            }

            // Certificates are stored in separate table and handled via ReplaceCertificatesAsync/Add/Update/Delete

            if (existingProfile.User != null && updatedProfile.User != null)
            {
                if (updatedProfile.User.FullName != null) existingProfile.User.FullName = updatedProfile.User.FullName;
                if (updatedProfile.User.SlugProfileUrl != null) existingProfile.User.SlugProfileUrl = updatedProfile.User.SlugProfileUrl;
                if (updatedProfile.User.Email != null) existingProfile.User.Email = updatedProfile.User.Email;
                if (updatedProfile.User.ProfilePicture != null) existingProfile.User.ProfilePicture = updatedProfile.User.ProfilePicture;
            }
            existingProfile.CurrentJobTitle ??= string.Empty;
            existingProfile.Bio ??= string.Empty;
            existingProfile.BankBinNumber ??= string.Empty;
            existingProfile.BankAccountNumber ??= string.Empty;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while saving coach profile changes. " + ex.ToString(), ex);
            }
        }

        public async Task ReplaceWorkExperiencesAsync(Guid coachId, IEnumerable<CoachWorkExperience> workExperiences)
        {
            var existingProfile = await _context.CoachProfiles
                .Include(p => p.WorkExperiences)
                .FirstOrDefaultAsync(p => p.Id == coachId);

            if (existingProfile == null)
                throw new Exception("Coach profile not found.");

            _context.Set<CoachWorkExperience>().RemoveRange(existingProfile.WorkExperiences);

            var items = workExperiences?.ToList() ?? new List<CoachWorkExperience>();
            foreach (var item in items)
            {
                item.CoachProfileId = coachId;
            }

            await _context.Set<CoachWorkExperience>().AddRangeAsync(items);
            await _context.SaveChangesAsync();
        }

        public async Task ReplaceCertificatesAsync(Guid coachId, IEnumerable<CoachCertificate> certificates)
        {
            var existingProfile = await _context.CoachProfiles
                .Include(p => p.Certificates)
                .FirstOrDefaultAsync(p => p.Id == coachId);

            if (existingProfile == null)
                throw new Exception("Coach profile not found.");

            _context.Set<CoachCertificate>().RemoveRange(existingProfile.Certificates);

            var items = certificates?.ToList() ?? new List<CoachCertificate>();
            foreach (var item in items)
            {
                item.CoachProfileId = coachId;
            }

            await _context.Set<CoachCertificate>().AddRangeAsync(items);
            await _context.SaveChangesAsync();
        }

        public async Task<CoachCertificate> AddCoachCertificateAsync(CoachCertificate certificate)
        {
            await _context.Set<CoachCertificate>().AddAsync(certificate);
            await _context.SaveChangesAsync();
            return certificate;
        }

        public async Task UpdateCoachCertificateAsync(CoachCertificate certificate)
        {
            var existing = await _context.Set<CoachCertificate>()
                .FirstOrDefaultAsync(x => x.Id == certificate.Id);

            if (existing == null)
                throw new Exception("Certificate not found.");

            existing.CoachProfileId = certificate.CoachProfileId;
            existing.Name = certificate.Name;
            existing.Issuer = certificate.Issuer;
            existing.IssuedAt = certificate.IssuedAt;
            existing.ExpiryAt = certificate.ExpiryAt;
            existing.Link = certificate.Link;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteCoachCertificateAsync(Guid certificateId)
        {
            var cert = await _context.Set<CoachCertificate>().FindAsync(certificateId);
            if (cert != null)
            {
                _context.Set<CoachCertificate>().Remove(cert);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<CoachWorkExperience> AddWorkExperienceAsync(CoachWorkExperience workExperience)
        {
            await _context.Set<CoachWorkExperience>().AddAsync(workExperience);
            await _context.SaveChangesAsync();
            return workExperience;
        }

        public async Task UpdateWorkExperienceAsync(CoachWorkExperience workExperience)
        {
            var existing = await _context.Set<CoachWorkExperience>()
                .FirstOrDefaultAsync(x => x.Id == workExperience.Id);

            if (existing == null)
                throw new Exception("Work experience not found.");

            existing.CoachProfileId = workExperience.CoachProfileId;
            existing.CompanyName = workExperience.CompanyName;
            existing.PositionTitle = workExperience.PositionTitle;
            existing.JobType = workExperience.JobType;
            existing.Location = workExperience.Location;
            existing.LocationType = workExperience.LocationType;
            existing.StartDate = workExperience.StartDate;
            existing.EndDate = workExperience.EndDate;
            existing.IsCurrentWorking = workExperience.IsCurrentWorking;
            existing.IsEnded = workExperience.IsEnded;
            existing.Description = workExperience.Description;
            existing.SkillIds = workExperience.SkillIds ?? new List<Guid>();

            await _context.SaveChangesAsync();
        }

        public async Task DeleteWorkExperienceAsync(Guid workExperienceId)
        {
            var exp = await _context.Set<CoachWorkExperience>().FindAsync(workExperienceId);
            if (exp != null)
            {
                _context.Set<CoachWorkExperience>().Remove(exp);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetTotalCoachCountAsync()
        {
            return await _context.CoachProfiles.CountAsync();
        }
    }
}
