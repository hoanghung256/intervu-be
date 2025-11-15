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

        public async Task CreateInterviewerProfile(InterviewerCreateDto dto)
        {
            var profile = new InterviewerProfile
            {
                Id = dto.Id,
                CurrentAmount = 0,
                ExperienceYears = dto.ExperienceYears,
                Status = dto.Status,
                Companies = [],
                Skills = []
            };

            if (dto.CompanyIds?.Any() == true)
            {
                var companies = await _context.Companies
                    .Where(c => dto.CompanyIds.Contains(c.Id))
                    .ToListAsync();

                profile.Companies = companies;
            }

            if (dto.SkillIds?.Any() == true)
            {
                var skills = await _context.Skills
                    .Where(s => dto.SkillIds.Contains(s.Id))
                    .ToListAsync();

                profile.Skills = skills;
            }

            await _context.InterviewerProfiles.AddAsync(profile);
            await _context.SaveChangesAsync();
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

        public async Task UpdateInterviewerProfileAsync(InterviewerUpdateDto updatedProfile)
        {
            var existingProfile = await _context.InterviewerProfiles
                .Include(p => p.Companies)
                .Include(p => p.Skills)
                .FirstOrDefaultAsync(p => p.Id == updatedProfile.Id);

            if (existingProfile == null)
                throw new Exception("Interviewer profile not found.");

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == updatedProfile.Id);

            if (existingUser == null)
                throw new Exception("User not found.");

            existingProfile.PortfolioUrl = updatedProfile.PortfolioUrl;
            existingProfile.CurrentAmount = updatedProfile.CurrentAmount;
            existingProfile.ExperienceYears = updatedProfile.ExperienceYears;
            existingProfile.Bio = updatedProfile.Bio;


            var newCompanies = await _context.Companies
                .Where(c => updatedProfile.CompanyIds.Contains(c.Id))
                .ToListAsync();

            existingProfile.Companies.Clear();
            foreach (var c in newCompanies)
                existingProfile.Companies.Add(c);


            var newSkills = await _context.Skills
                .Where(s => updatedProfile.SkillIds.Contains(s.Id))
                .ToListAsync();

            existingProfile.Skills.Clear();
            foreach (var s in newSkills)
                existingProfile.Skills.Add(s);


            existingUser.FullName = updatedProfile.FullName;
            existingUser.Email = updatedProfile.Email;
            existingUser.ProfilePicture = updatedProfile.ProfilePicture;

            await _context.SaveChangesAsync();
        }

    }
}
