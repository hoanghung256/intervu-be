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
                    Title = q.Title,
                    Content = q.Content,
                    InterviewExperienceId = experience.Id,
                    Level = q.Level,
                    Round = q.Round,
                    Category = q.Category,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                foreach (var cid in q.CompanyIds)
                    question.QuestionCompanies.Add(new Domain.Entities.QuestionCompany { QuestionId = question.Id, CompanyId = cid });
                foreach (var r in q.Roles)
                    question.QuestionRoles.Add(new Domain.Entities.QuestionRole { QuestionId = question.Id, Role = r });
                foreach (var tid in q.TagIds)
                    question.QuestionTags.Add(new Domain.Entities.QuestionTag { QuestionId = question.Id, TagId = tid });

                if (!string.IsNullOrWhiteSpace(q.Answer))
                {
                    question.Answers.Add(new Domain.Entities.Answer
                    {
                        Id = Guid.NewGuid(),
                        QuestionId = question.Id,
                        AuthorId = userId,
                        Content = q.Answer,
                        Upvotes = 0,
                        IsVerified = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
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
