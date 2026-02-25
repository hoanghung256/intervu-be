using AutoMapper;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.InterviewExperience;
using Intervu.Application.Interfaces.UseCases.InterviewExperience;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.InterviewExperience
{
    public class GetInterviewExperiences(IInterviewExperienceRepository repository, IMapper mapper)
        : IGetInterviewExperiences
    {
        private const int PageSize = 10;

        public async Task<PagedResult<InterviewExperienceSummaryDto>> ExecuteAsync(InterviewExperienceFilterRequest filter)
        {
            if (filter.Page < 1) filter.Page = 1;
            var (items, total) = await repository.GetPagedAsync(
                filter.SearchTerm, filter.Role, filter.Level, filter.LastRoundCompleted, filter.Page, PageSize);
            var dtos = items.Select(e =>
            {
                var dto = mapper.Map<InterviewExperienceSummaryDto>(e);
                dto.QuestionCount = e.Questions?.Count ?? 0;
                return dto;
            }).ToList();
            return new PagedResult<InterviewExperienceSummaryDto>(dtos, total, PageSize, filter.Page);
        }
    }

    public class GetInterviewExperienceDetail(IInterviewExperienceRepository repository, IMapper mapper)
        : IGetInterviewExperienceDetail
    {
        public async Task<InterviewExperienceDetailDto?> ExecuteAsync(Guid id)
        {
            var entity = await repository.GetDetailAsync(id);
            if (entity == null) return null;
            var dto = mapper.Map<InterviewExperienceDetailDto>(entity);
            dto.QuestionCount = entity.Questions?.Count ?? 0;
            return dto;
        }
    }

    public class CreateInterviewExperience(IUnitOfWork unitOfWork, IMapper mapper)
        : ICreateInterviewExperience
    {
        public async Task<Guid> ExecuteAsync(CreateInterviewExperienceRequest request, Guid userId)
        {
            var repo = unitOfWork.GetRepository<IInterviewExperienceRepository>();

            var experience = mapper.Map<Domain.Entities.InterviewExperience>(request);
            experience.Id = Guid.NewGuid();
            experience.CreatedBy = userId;
            experience.UpdatedBy = userId;
            experience.CreatedAt = DateTime.UtcNow;
            experience.UpdatedAt = DateTime.UtcNow;

            foreach (var q in request.Questions)
            {
                experience.Questions.Add(new Question
                {
                    Id = Guid.NewGuid(),
                    InterviewExperienceId = experience.Id,
                    QuestionType = q.QuestionType,
                    Content = q.Content,
                    Answer = q.Answer,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await repo.AddAsync(experience);
            await unitOfWork.SaveChangesAsync();

            return experience.Id;
        }
    }

    public class UpdateInterviewExperience(IUnitOfWork unitOfWork, IMapper mapper)
        : IUpdateInterviewExperience
    {
        public async Task ExecuteAsync(Guid id, UpdateInterviewExperienceRequest request, Guid userId)
        {
            var repo = unitOfWork.GetRepository<IInterviewExperienceRepository>();
            var experience = await repo.GetByIdAsync(id)
                ?? throw new Exception("Interview experience not found");

            experience.CompanyName = request.CompanyName;
            experience.Role = request.Role;
            experience.Level = request.Level;
            experience.LastRoundCompleted = request.LastRoundCompleted;
            experience.InterviewProcess = request.InterviewProcess;
            experience.IsInterestedInContact = request.IsInterestedInContact;
            experience.UpdatedBy = userId;
            experience.UpdatedAt = DateTime.UtcNow;

            repo.UpdateAsync(experience);
            await unitOfWork.SaveChangesAsync();
        }
    }

    public class DeleteInterviewExperience(IUnitOfWork unitOfWork)
        : IDeleteInterviewExperience
    {
        public async Task ExecuteAsync(Guid id, Guid userId)
        {
            var repo = unitOfWork.GetRepository<IInterviewExperienceRepository>();
            var experience = await repo.GetByIdAsync(id)
                ?? throw new Exception("Interview experience not found");

            repo.DeleteAsync(experience);
            await unitOfWork.SaveChangesAsync();
        }
    }

    public class AddQuestion(IUnitOfWork unitOfWork) : IAddQuestion
    {
        public async Task<Guid> ExecuteAsync(Guid experienceId, CreateQuestionRequest request)
        {
            var questionRepo = unitOfWork.GetRepository<IQuestionRepository>();
            var question = new Question
            {
                Id = Guid.NewGuid(),
                InterviewExperienceId = experienceId,
                QuestionType = request.QuestionType,
                Content = request.Content,
                Answer = request.Answer,
                CreatedAt = DateTime.UtcNow
            };

            await questionRepo.AddAsync(question);
            await unitOfWork.SaveChangesAsync();

            return question.Id;
        }
    }

    public class UpdateQuestion(IUnitOfWork unitOfWork) : IUpdateQuestion
    {
        public async Task ExecuteAsync(Guid questionId, UpdateQuestionRequest request, Guid userId)
        {
            var questionRepo = unitOfWork.GetRepository<IQuestionRepository>();
            var question = await questionRepo.GetByIdAsync(questionId)
                ?? throw new Exception("Question not found");

            question.QuestionType = request.QuestionType;
            question.Content = request.Content;
            question.Answer = request.Answer;

            questionRepo.UpdateAsync(question);
            await unitOfWork.SaveChangesAsync();
        }
    }

    public class DeleteQuestion(IUnitOfWork unitOfWork) : IDeleteQuestion
    {
        public async Task ExecuteAsync(Guid questionId, Guid userId)
        {
            var questionRepo = unitOfWork.GetRepository<IQuestionRepository>();
            var question = await questionRepo.GetByIdAsync(questionId)
                ?? throw new Exception("Question not found");

            questionRepo.DeleteAsync(question);
            await unitOfWork.SaveChangesAsync();
        }
    }
}
