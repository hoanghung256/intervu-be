using Intervu.Application.DTOs.GeneratedQuestion;
using Intervu.Application.Interfaces.UseCases.GeneratedQuestion;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.GeneratedQuestion
{
    public class CreateGeneratedQuestion(IUnitOfWork unitOfWork) : ICreateGeneratedQuestion
    {
        public async Task<Guid> ExecuteAsync(CreateGeneratedQuestionRequest request, Guid creatorUserId)
        {
            var generatedRepo = unitOfWork.GetRepository<IGeneratedQuestionRepository>();
            var tagRepo = unitOfWork.GetRepository<ITagRepository>();

            var item = new Domain.Entities.GeneratedQuestion
            {
                Id = Guid.NewGuid(),
                InterviewRoomId = request.InterviewRoomId,
                Title = request.Title?.Trim() ?? string.Empty,
                Content = request.Content.Trim(),
                Status = GeneratedQuestionStatus.PendingReview
            };

            if (request.Tags != null && request.Tags.Any())
            {
                var dbTags = await tagRepo.GetAllAsync();
                var tagMap = dbTags.ToDictionary(t => t.Name, t => t.Id, StringComparer.OrdinalIgnoreCase);

                var matchingTagIds = new System.Collections.Generic.List<Guid>();
                foreach (var tagName in request.Tags)
                {
                    if (!string.IsNullOrWhiteSpace(tagName))
                    {
                        var normalizedName = tagName.Trim();
                        if (tagMap.TryGetValue(normalizedName, out var existingId))
                        {
                            matchingTagIds.Add(existingId);
                        }
                        else
                        {
                            var newTag = new Domain.Entities.Tag { Id = Guid.NewGuid(), Name = normalizedName };
                            await tagRepo.AddAsync(newTag);
                            matchingTagIds.Add(newTag.Id);
                            tagMap[normalizedName] = newTag.Id;
                        }
                    }
                }

                item.TagIds = matchingTagIds;
            }

            await generatedRepo.AddAsync(item);
            await unitOfWork.SaveChangesAsync();

            return item.Id;
        }
    }
}
