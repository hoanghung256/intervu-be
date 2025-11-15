using Intervu.Application.Common;
using Intervu.Application.DTOs.Interviewer;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Domain.Entities;
using Intervu.Infrastructure.Persistence.SqlServer.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.Persistence.SqlServer
{
    public class InterviewerProfileRepository : RepositoryBase<InterviewerProfile>, IInterviewerProfileRepository
    {
        public InterviewerProfileRepository(IntervuDbContext context) : base(context)
        {
        }

        public async Task CreateInterviewerProfile(InterviewerProfile interviewerProfile)
        {
            throw new NotImplementedException();
        }

        public void DeleteInterviewerProfile(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<InterviewerProfile> GetProfileAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<InterviewerProfile?> GetProfileByIdAsync(int id)
        {
            InterviewerProfile? profile = await _context.InterviewerProfiles
                .Where(p => p.Id == id)
                .Include(p => p.Companies)
                .Include(p => p.Skills)
                .FirstOrDefaultAsync();

            return profile;
        }

        public async Task<PagedResult<InterviewerProfile>> GetPagedInterviewerProfilesAsync(GetInterviewerFilterRequest request)
        {
            var query = _context.InterviewerProfiles.AsQueryable()
                .Include(i => i.Companies)
                .Include(i => i.Skills)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(i => i.Bio.Contains(request.Search));
            }

            if (request.SkillId.HasValue)
            {
                query = query.Where(x => x.Skills.Any(c => c.Id == request.SkillId.Value));
            }

            if (request.CompanyId.HasValue)
            {
                query = query.Where(x => x.Companies.Any(c => c.Id == request.CompanyId.Value));
            }

            var totalItems = query.Count();
            
            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PagedResult<InterviewerProfile>(items, totalItems, request.PageSize, request.Page);
        }
        public async Task<IEnumerable<InterviewerProfile>> GetAllInterviewerProfilesAsync()
        {
            var profiles = await _context.InterviewerProfiles
                .Include(p => p.Companies)
                .Include(p => p.Skills)
                .ToListAsync();
            return profiles;
        }

        public async Task UpdateInterviewerProfileAsync(InterviewerUpdateDto updatedProfile)
        {
            var existingProfile = await _context.InterviewerProfiles
                .FirstOrDefaultAsync(p => p.Id == updatedProfile.Id);

            if (existingProfile == null)
                throw new Exception("Interviewer profile not found.");

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == updatedProfile.Id);

            if (existingUser == null)
                throw new Exception("User not found.");

            existingProfile.PortfolioUrl = updatedProfile.PortfolioUrl;
            //existingProfile.Specializations = updatedProfile.Specializations;
            //existingProfile.ProgrammingLanguages = updatedProfile.ProgrammingLanguages;
            //existingProfile.Companies = updatedProfile.;
            existingProfile.CurrentAmount = updatedProfile.CurrentAmount;
            existingProfile.ExperienceYears = updatedProfile.ExperienceYears;
            existingProfile.Bio = updatedProfile.Bio;
            existingProfile.Status = updatedProfile.Status;

            existingUser.FullName = updatedProfile.FullName;
            existingUser.Email = updatedProfile.Email;
            existingUser.Password = updatedProfile.Password;
            existingUser.ProfilePicture = updatedProfile.ProfilePicture;

            await _context.SaveChangesAsync();
        }

    }
}
