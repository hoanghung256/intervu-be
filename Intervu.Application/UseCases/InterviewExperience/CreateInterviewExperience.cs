using AutoMapper;
using Intervu.Application.DTOs.InterviewExperience;
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
            var questionRepo = unitOfWork.GetRepository<IQuestionRepository>();
            var commentRepo = unitOfWork.GetRepository<ICommentRepository>();

            var company = await companyRepo.GetByIdAsync(request.CompanyId);
            if (company is null)
                throw new NotFoundException($"Company with id '{request.CompanyId}' was not found.");

            var now = DateTime.UtcNow;

            var experience = mapper.Map<Domain.Entities.InterviewExperience>(request);
            experience.Id = Guid.NewGuid();
            experience.CreatedBy = userId;
            experience.UpdatedBy = userId;
            experience.CreatedAt = now;
            experience.UpdatedAt = now;

            foreach (var q in request.Questions)
            {
                if (q.LinkedQuestionId.HasValue)
                {
                    _ = await questionRepo.GetByIdAsync(q.LinkedQuestionId.Value)
                        ?? throw new NotFoundException($"Question '{q.LinkedQuestionId}' was not found.");

                    if (!string.IsNullOrWhiteSpace(q.Answer))
                    {
                        await commentRepo.AddAsync(new Domain.Entities.Comment
                        {
                            Id = Guid.NewGuid(),
                            QuestionId = q.LinkedQuestionId.Value,
                            Content = q.Answer,
                            IsAnswer = true,
                            Vote = 0,
                            CreatedAt = now,
                            UpdateAt = now,
                            CreateBy = userId,
                            UpdateBy = userId
                        });
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(q.Title))
                        throw new ArgumentException("Title is required when creating a new question.");
                    if (string.IsNullOrWhiteSpace(q.Content))
                        throw new ArgumentException("Content is required when creating a new question.");

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
                        CreatedAt = now,
                        UpdatedAt = now
                    };

                    foreach (var cid in q.CompanyIds)
                        question.QuestionCompanies.Add(new QuestionCompany { CompanyId = cid });

                    foreach (var r in q.Roles)
                        question.QuestionRoles.Add(new QuestionRole { Role = r });

                    foreach (var tid in q.TagIds)
                        question.QuestionTags.Add(new QuestionTag { TagId = tid });

                    if (!string.IsNullOrWhiteSpace(q.Answer))
                    {
                        question.Comments.Add(new Domain.Entities.Comment
                        {
                            Id = Guid.NewGuid(),
                            QuestionId = question.Id,
                            Content = q.Answer,
                            IsAnswer = true,
                            Vote = 0,
                            CreatedAt = now,
                            UpdateAt = now,
                            CreateBy = userId,
                            UpdateBy = userId
                        });
                    }

                    experience.Questions.Add(question);
                }
            }

            await repo.AddAsync(experience);
            await unitOfWork.SaveChangesAsync();

            return experience.Id;
        }
    }
}
