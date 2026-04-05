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
                .Include(p => p.Certificates)
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
                .Include(p => p.Certificates)
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
                var skillIds = updatedProfile.Skills.Select(s => s.Id).Distinct().ToList();
                if (skillIds.Count > 0)
                {
                    var skills = await _context.Skills.Where(s => skillIds.Contains(s.Id)).ToListAsync();
                    foreach (var skill in skills)
                    {
                        existingProfile.Skills.Add(skill);
                    }
                }
            }

            if (updatedProfile.Industries != null)
            {
                var industryIds = updatedProfile.Industries.Select(i => i.Id).Distinct().ToList();
                if (industryIds.Count > 0)
                {
                    var industries = await _context.Industries.Where(i => industryIds.Contains(i.Id)).ToListAsync();
                    foreach (var industry in industries)
                    {
                        existingProfile.Industries.Add(industry);
                    }
                }
            }

            if (existingProfile.User != null && updatedProfile.User != null)
            {
                if (!string.IsNullOrWhiteSpace(updatedProfile.User.FullName))
                    existingProfile.User.FullName = updatedProfile.User.FullName;
                if (!string.IsNullOrWhiteSpace(updatedProfile.User.SlugProfileUrl))
                    existingProfile.User.SlugProfileUrl = updatedProfile.User.SlugProfileUrl;
                if (!string.IsNullOrWhiteSpace(updatedProfile.User.Email))
                    existingProfile.User.Email = updatedProfile.User.Email;
                if (updatedProfile.User.ProfilePicture != null)
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

        public async Task<CandidateWorkExperience> AddWorkExperienceAsync(CandidateWorkExperience workExperience)
        {
            await _context.Set<CandidateWorkExperience>().AddAsync(workExperience);
            await _context.SaveChangesAsync();
            return workExperience;
        }

        public async Task UpdateWorkExperienceAsync(CandidateWorkExperience workExperience)
        {
            var existing = await _context.Set<CandidateWorkExperience>()
                .FirstOrDefaultAsync(x => x.Id == workExperience.Id);

            if (existing == null)
                throw new Exception("Work experience not found.");

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
            var we = await _context.Set<CandidateWorkExperience>().FindAsync(workExperienceId);
            if (we != null)
            {
                _context.Set<CandidateWorkExperience>().Remove(we);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ReplaceCertificatesAsync(Guid candidateId, IEnumerable<CandidateCertificate> certificates)
        {
            var existingProfile = await _context.CandidateProfiles
                .Include(p => p.Certificates)
                .FirstOrDefaultAsync(p => p.Id == candidateId);

            if (existingProfile == null)
                throw new Exception("Candidate profile not found.");

            _context.Set<CandidateCertificate>().RemoveRange(existingProfile.Certificates);

            var items = certificates?.ToList() ?? new List<CandidateCertificate>();
            foreach (var item in items)
            {
                item.CandidateProfileId = candidateId;
            }

            await _context.Set<CandidateCertificate>().AddRangeAsync(items);
            await _context.SaveChangesAsync();
        }

        public async Task<CandidateCertificate> AddCandidateCertificateAsync(CandidateCertificate certificate)
        {
            await _context.Set<CandidateCertificate>().AddAsync(certificate);
            await _context.SaveChangesAsync();
            return certificate;
        }

        public async Task UpdateCandidateCertificateAsync(CandidateCertificate certificate)
        {
            var existing = await _context.Set<CandidateCertificate>()
                .FirstOrDefaultAsync(x => x.Id == certificate.Id);

            if (existing == null)
                throw new Exception("Certificate not found.");

            existing.CandidateProfileId = certificate.CandidateProfileId;
            existing.Name = certificate.Name;
            existing.Issuer = certificate.Issuer;
            existing.IssuedAt = certificate.IssuedAt;
            existing.ExpiryAt = certificate.ExpiryAt;
            existing.Link = certificate.Link;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteCandidateCertificateAsync(Guid certificateId)
        {
            var cert = await _context.Set<CandidateCertificate>().FindAsync(certificateId);
            if (cert != null)
            {
                _context.Set<CandidateCertificate>().Remove(cert);
                await _context.SaveChangesAsync();
            }
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
