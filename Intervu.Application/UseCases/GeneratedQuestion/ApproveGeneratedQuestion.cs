using Intervu.Application.DTOs.GeneratedQuestion;
using Intervu.Application.Interfaces.UseCases.Audit;
using Intervu.Application.Interfaces.UseCases.GeneratedQuestion;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.GeneratedQuestion
{
    public class ApproveGeneratedQuestion(
        IUnitOfWork unitOfWork,
        IAddAuditLogEntry addAuditLogEntry) : IApproveGeneratedQuestion
    {
        public async Task<Guid> ExecuteAsync(Guid generatedQuestionId, ApproveGeneratedQuestionRequest request, Guid reviewerUserId)
        {
            var generatedRepo = unitOfWork.GetRepository<IGeneratedQuestionRepository>();
            var questionRepo = unitOfWork.GetRepository<IQuestionRepository>();
            var tagRepo = unitOfWork.GetRepository<ITagRepository>();

            var generated = await generatedRepo.GetByIdAsync(generatedQuestionId)
                ?? throw new Exception("Generated question not found");

            if (generated.Status != GeneratedQuestionStatus.PendingReview)
            {
                throw new Exception("Generated question is already reviewed");
            }

            // Prepare Audit Log data before modification
            var auditMetaData = new
            {
                GeneratedQuestionId = generatedQuestionId,
                Original = new
                {
                    Title = generated.Title,
                    Content = generated.Content,
                    TagIds = generated.TagIds
                },
                Approved = new
                {
                    Title = request.Title?.Trim() ?? generated.Title,
                    Content = request.Content?.Trim() ?? generated.Content,
                    TagIds = request.TagIds,
                    Tags = request.Tags
                }
            };

            var now = DateTime.UtcNow;
            var questionId = Guid.NewGuid();
            var question = new Domain.Entities.Question
            {
                Id = questionId,
                Title = string.IsNullOrWhiteSpace(request.Title) ? generated.Title : request.Title.Trim(),
                Content = string.IsNullOrWhiteSpace(request.Content) ? generated.Content : request.Content.Trim(),
                InterviewExperienceId = request.InterviewExperienceId,
                Level = request.Level,
                Round = request.Round,
                Category = request.Category,
                CreatedBy = reviewerUserId,
                CreatedAt = now,
                UpdatedAt = now
            };

            foreach (var cid in request.CompanyIds)
                question.QuestionCompanies.Add(new QuestionCompany { QuestionId = question.Id, CompanyId = cid });

            foreach (var role in request.Roles)
                question.QuestionRoles.Add(new QuestionRole { QuestionId = question.Id, Role = role });

            var finalTagIds = new List<Guid>();

            if (request.Tags != null && request.Tags.Any())
            {
                var dbTags = await tagRepo.GetAllAsync();
                var tagMap = dbTags.ToDictionary(t => t.Name, t => t.Id, StringComparer.OrdinalIgnoreCase);

                foreach (var tagName in request.Tags)
                {
                    if (!string.IsNullOrWhiteSpace(tagName))
                    {
                        var normalizedName = tagName.Trim();
                        if (tagMap.TryGetValue(normalizedName, out var existingId))
                        {
                            finalTagIds.Add(existingId);
                        }
                        else
                        {
                            var newTag = new Intervu.Domain.Entities.Tag { Id = Guid.NewGuid(), Name = normalizedName };
                            await tagRepo.AddAsync(newTag);
                            finalTagIds.Add(newTag.Id);
                            tagMap[normalizedName] = newTag.Id;
                        }
                    }
                }
            }
            else if (request.TagIds != null && request.TagIds.Any())
            {
                finalTagIds = request.TagIds;
            }
            else if (generated.TagIds != null && generated.TagIds.Any())
            {
                finalTagIds = generated.TagIds;
            }

            foreach (var tagId in finalTagIds)
                question.QuestionTags.Add(new QuestionTag { QuestionId = question.Id, TagId = tagId });

            await questionRepo.AddAsync(question);

            // Update original generated question to show it was approved
            generated.Title = question.Title;
            generated.Content = question.Content;
            generated.TagIds = finalTagIds;
            generated.Status = GeneratedQuestionStatus.Approved;
            generatedRepo.UpdateAsync(generated);

            await unitOfWork.SaveChangesAsync();

            // Log the approval and changes
            await addAuditLogEntry.ExecuteAsync(new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = reviewerUserId,
                EventType = AuditLogEventType.GeneratedQuestionApprove,
                Content = $"Approved and contributed generated question: {question.Title}",
                MetaData = JsonSerializer.Serialize(auditMetaData),
                Timestamp = DateTime.UtcNow
            });

            return questionId;
        }
    }
}
