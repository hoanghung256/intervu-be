using AutoMapper;
using Intervu.Application.DTOs.InterviewExperience;
using Intervu.Application.DTOs.Question;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.InterviewExperience;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.InterviewExperience
{
    public class CreateInterviewExperience(IUnitOfWork unitOfWork, IMapper mapper)
        : ICreateInterviewExperience
    {
        public async Task<Guid> ExecuteAsync(CreateInterviewExperienceRequest request, Guid userId)
        {
            var repo = unitOfWork.GetRepository<IInterviewExperienceRepository>();

            var companyRepo = unitOfWork.GetRepository<ICompanyRepository>();
            var company = await companyRepo.GetByIdAsync(request.CompanyId);
            if (company is null)
                throw new NotFoundException($"Company with id '{request.CompanyId}' was not found.");

            var experience = mapper.Map<Domain.Entities.InterviewExperience>(request);
            experience.Id = Guid.NewGuid();
            experience.CreatedBy = userId;
            experience.UpdatedBy = userId;
            experience.CreatedAt = DateTime.UtcNow;
            experience.UpdatedAt = DateTime.UtcNow;

            foreach (var q in request.Questions)
            {
                var question = new Domain.Entities.Question
                {
                    Id = Guid.NewGuid(),
                    InterviewExperienceId = experience.Id,
                    QuestionType = q.QuestionType,
                    Content = q.Content,
                    Answer = q.Answer,
                    CreatedAt = DateTime.UtcNow
                };

                if (!string.IsNullOrWhiteSpace(q.Answer))
                {
                    question.Comments.Add(new Domain.Entities.Comment
                    {
                        Id = Guid.NewGuid(),
                        QuestionId = question.Id,
                        Content = q.Answer,
                        IsAnswer = true,
                        Vote = 0,
                        CreatedAt = DateTime.UtcNow,
                        UpdateAt = DateTime.UtcNow,
                        CreateBy = userId,
                        UpdateBy = userId
                    });
                }

                experience.Questions.Add(question);
            }

            await repo.AddAsync(experience);
            await unitOfWork.SaveChangesAsync();

            return experience.Id;
        }
    }
}
