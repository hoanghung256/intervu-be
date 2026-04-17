using AutoMapper;
using Intervu.Application.DTOs.Candidate;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.CandidateProfile;
using Intervu.Application.Utils;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.CandidateProfile
{
    public class UpdateCandidateProfile : IUpdateCandidateProfile
    {
        private readonly ICandidateProfileRepository _repo;
        private readonly IMapper _mapper;
        private readonly ISkillRepository _skillRepository;
        private readonly IIndustryRepository _industryRepository;
        private readonly IBankFieldProtector _bankFieldProtector;

        public UpdateCandidateProfile(
            ICandidateProfileRepository repo,
            ISkillRepository skillRepository,
            IIndustryRepository industryRepository,
            IMapper mapper,
            IBankFieldProtector bankFieldProtector)
        {
            _repo = repo;
            _skillRepository = skillRepository;
            _industryRepository = industryRepository;
            _mapper = mapper;
            _bankFieldProtector = bankFieldProtector;
        }

        public async Task<CandidateProfileDto> UpdateCandidateProfileAsync(Guid id, CandidateUpdateDto updateDto)
        {
            var existing = await _repo.GetProfileByIdAsync(id);
            if (existing == null)
                throw new Exception("Candidate profile not found.");

            _mapper.Map(updateDto, existing);

            // Always keep profile id from route to avoid accidental Guid.Empty from payload
            existing.Id = id;

            if (updateDto.BankAccountNumber != null)
            {
                var plain = updateDto.BankAccountNumber.Trim();
                if (plain.Length == 0)
                {
                    existing.BankAccountNumber = string.Empty;
                    existing.BankAccountNumberMasked = string.Empty;
                }
                else
                {
                    existing.BankAccountNumber = _bankFieldProtector.Encrypt(plain);
                    existing.BankAccountNumberMasked = _bankFieldProtector.Mask(plain);
                }
            }

            if (existing.User != null)
            {
                if (!string.IsNullOrWhiteSpace(updateDto.FullName))
                {
                    existing.User.FullName = updateDto.FullName;
                    existing.User.SlugProfileUrl = SlugProfileUrlHandler.GenerateProfileSlug(updateDto.FullName);
                }

                if (!string.IsNullOrWhiteSpace(updateDto.Email))
                    existing.User.Email = updateDto.Email;

                if (updateDto.ProfilePicture != null)
                    existing.User.ProfilePicture = updateDto.ProfilePicture;
            }

            if (updateDto.SkillIds != null)
            {
                var skills = await _skillRepository.GetByIdsAsync(updateDto.SkillIds);
                existing.Skills = skills.ToList();
            }

            if (updateDto.IndustryIds != null)
            {
                var industries = await _industryRepository.GetByIdsAsync(updateDto.IndustryIds);
                existing.Industries = industries.ToList();
            }

            await _repo.UpdateCandidateProfileAsync(existing);

            var reloaded = await _repo.GetProfileByIdAsync(id);
            return _mapper.Map<CandidateProfileDto>(reloaded!);
        }

        public async Task<CandidateProfileDto> UpdateCandidateWorkExperiencesAsync(Guid id, List<CandidateWorkExperienceDto> workExperiences)
        {
            var existing = await _repo.GetProfileByIdAsync(id);
            if (existing == null)
                throw new Exception("Candidate profile not found.");

            var entities = (workExperiences ?? new List<CandidateWorkExperienceDto>())
                .Select(x => new CandidateWorkExperience
                {
                    Id = Guid.NewGuid(),
                    CandidateProfileId = id,
                    CompanyName = x.CompanyName,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    IsCurrentWorking = x.IsCurrentWorking,
                    IsEnded = x.IsEnded,
                    Description = x.Description,
                    SkillIds = x.SkillIds ?? new List<Guid>()
                })
                .ToList();

            await _repo.ReplaceWorkExperiencesAsync(id, entities);

            var reloaded = await _repo.GetProfileByIdAsync(id);
            return _mapper.Map<CandidateProfileDto>(reloaded!);
        }

        public async Task<CandidateViewDto> UpdateCandidateStatusAsync(Guid id, UserStatus status)
        {
            var profile = await _repo.GetByIdAsync(id);
            if (profile == null)
                throw new Exception("Candidate profile not found!");

            profile.User.Status = status;
            await _repo.SaveChangesAsync();

            return _mapper.Map<CandidateViewDto>(profile);
         }

        async Task<Domain.Entities.CandidateProfile> IUpdateCandidateProfile.UpdateCandidateCVProfile(Guid id, string cvUrl)
        {
            Domain.Entities.CandidateProfile profile = await _repo.GetByIdAsync(id);
            if (profile == null) {
                return null;
            }
            profile.CVUrl = cvUrl;

            _repo.UpdateAsync(profile);
            await _repo.SaveChangesAsync();

            return profile;

        }
    }
}
