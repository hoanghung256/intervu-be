using AutoMapper;
using Intervu.Application.Common;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Admin
{
    public class GetAllInterviewersForAdmin : IGetAllInterviewersForAdmin
    {
        private readonly IInterviewerProfileRepository _interviewerProfileRepository;
        private readonly IUserRepository _userRepository;

        public GetAllInterviewersForAdmin(
            IInterviewerProfileRepository interviewerProfileRepository,
            IUserRepository userRepository)
        {
            _interviewerProfileRepository = interviewerProfileRepository;
            _userRepository = userRepository;
        }

        public async Task<PagedResult<InterviewerAdminDto>> ExecuteAsync(int page, int pageSize)
        {
            var pagedInterviewers = await _interviewerProfileRepository.GetPagedInterviewerProfilesAsync(page, pageSize);

            var interviewerDtos = new List<InterviewerAdminDto>();

            foreach (var interviewer in pagedInterviewers.Items)
            {
                var user = await _userRepository.GetByIdAsync(interviewer.Id);
                var skills = interviewer.Skills?.Select(s => s.Name).ToList() ?? new List<string>();
                var specialization = skills.Any() ? string.Join(", ", skills) : null;

                interviewerDtos.Add(new InterviewerAdminDto
                {
                    Id = interviewer.Id,
                    FullName = user?.FullName ?? "-",
                    Email = user?.Email ?? "-",
                    Phone = null, // User entity doesn't have PhoneNumber field
                    Specialization = specialization,
                    Experience = interviewer.ExperienceYears ?? 0, // Default to 0 if null
                    Bio = interviewer.Bio,
                    HourlyRate = interviewer.CurrentAmount ?? 0, // Default to 0 if null
                    CreatedAt = DateTime.UtcNow // Placeholder since EntityBase doesn't have CreatedAt
                });
            }

            return new PagedResult<InterviewerAdminDto>(interviewerDtos, pagedInterviewers.TotalItems, pageSize, page);
        }
    }
}
