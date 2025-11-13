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

        public async Task<InterviewerProfileDto> GetProfileByIdAsync(int id)
        {
            var profile = await _context.InterviewerProfiles
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profile == null)
                throw new Exception("Interviewer profile not found.");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                throw new Exception("User not found.");

            return new InterviewerProfileDto
            {
                Id = profile.Id,
                FullName = user.FullName,
                Email = user.Email,
                ProfilePicture = user.ProfilePicture,
                CVUrl = profile.CVUrl,
                PortfolioUrl = profile.PortfolioUrl,
                Specializations = profile.Specializations,
                ProgrammingLanguages = profile.ProgrammingLanguages,
                Company = profile.Company,
                CurrentAmount = profile.CurrentAmount,
                ExperienceYears = profile.ExperienceYears,
                Bio = profile.Bio,
                Status = profile.Status
            };
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
            existingProfile.Specializations = updatedProfile.Specializations;
            existingProfile.ProgrammingLanguages = updatedProfile.ProgrammingLanguages;
            existingProfile.Company = updatedProfile.Company;
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
