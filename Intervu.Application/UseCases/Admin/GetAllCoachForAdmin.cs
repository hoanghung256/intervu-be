using AutoMapper;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Admin;
using Intervu.Domain.Repositories;
using Intervu.Application.Interfaces.UseCases.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Admin
{
    public class GetAllCoachForAdmin : IGetAllCoachForAdmin
    {
        private readonly ICoachProfileRepository _coachProfileRepository;
        private readonly IUserRepository _userRepository;

        public GetAllCoachForAdmin(
            ICoachProfileRepository coachProfileRepository,
            IUserRepository userRepository)
        {
            _coachProfileRepository = coachProfileRepository;
            _userRepository = userRepository;
        }

        public async Task<PagedResult<CoachAdminDto>> ExecuteAsync(int page, int pageSize)
        {
            var pagedCoach = await _coachProfileRepository.GetPagedCoachProfilesAsync(
                search: null,
                skillId: null,
                companyId: null,
                page: page,
                pageSize: pageSize
            );

            var coachDtos = new List<CoachAdminDto>();

            foreach (var coach in pagedCoach.Items)
            {
                var user = await _userRepository.GetByIdAsync(coach.Id);
                var skills = coach.Skills?.Select(s => s.Name).ToList() ?? new List<string>();
                var specialization = skills.Any() ? string.Join(", ", skills) : null;

                coachDtos.Add(new CoachAdminDto
                {
                    Id = coach.Id,
                    FullName = user?.FullName ?? "-",
                    Email = user?.Email ?? "-",
                    Phone = null, // User entity doesn't have PhoneNumber field
                    Specialization = specialization,
                    Experience = coach.ExperienceYears ?? 0, // Default to 0 if null
                    Bio = coach.Bio,
                    HourlyRate = coach.CurrentAmount ?? 0, // Default to 0 if null
                    CreatedAt = DateTime.UtcNow // Placeholder since EntityBase doesn't have CreatedAt
                });
            }

            return new PagedResult<CoachAdminDto>(coachDtos, pagedCoach.TotalCount, pageSize, page);
        }
    }
}
