using AutoMapper;
using Intervu.Application.DTOs.Interviewer;
using Intervu.Application.Interfaces.UseCases.InterviewerProfile;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.InterviewerProfile
{
    public class UpdateInterviewerProfile : IUpdateInterviewProfile
    {
        private readonly IInterviewerProfileRepository _repo;
        private readonly ICompanyRepository _companyRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IMapper _mapper;

        public UpdateInterviewerProfile(IInterviewerProfileRepository repo, ICompanyRepository companyRepository, ISkillRepository skillRepository, IMapper mapper)
        {
            _repo = repo;
            _companyRepository = companyRepository;
            _skillRepository = skillRepository;
            _mapper = mapper;
        }

        public async Task<InterviewerProfileDto> UpdateInterviewProfile(Guid id, InterviewerUpdateDto interviewerUpdateDto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                return null;

            // Map Companies by IDs (DTO provides List<Guid> Companies)
            if (interviewerUpdateDto.CompanyIds != null)
            {
                var companies = await _companyRepository.GetByIdsAsync(interviewerUpdateDto.CompanyIds);
                existing.Companies = companies.ToList();
            }

            // Map Skills by IDs (DTO provides List<Guid> Skills)
            if (interviewerUpdateDto.SkillIds != null)
            {
                var skills = await _skillRepository.GetByIdsAsync(interviewerUpdateDto.SkillIds);
                existing.Skills = skills.ToList();
            }

            // Map simple properties from DTO to existing entity
            _mapper.Map(interviewerUpdateDto, existing);

            await _repo.UpdateInterviewerProfileAsync(existing);

            await _repo.SaveChangesAsync();

            return _mapper.Map<InterviewerProfileDto>(existing);
        }

        public async Task<InterviewerViewDto> UpdateInterviewStatus(Guid id, InterviewerProfileStatus status)
        {
            Domain.Entities.InterviewerProfile? profile = await _repo.GetByIdAsync(id);
            if (profile == null)
                throw new Exception("Profile not found!");
            profile.Status = status;
            await _repo.SaveChangesAsync();
            return _mapper.Map<InterviewerViewDto>(profile);
        }
    }
}
