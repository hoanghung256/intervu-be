using Intervu.Application.DTOs.GeneratedQuestion;
using Intervu.Application.Interfaces.UseCases.GeneratedQuestion;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.GeneratedQuestion
{
    public class GetGeneratedQuestionsByRoom(
        IGeneratedQuestionRepository generatedQuestionRepository,
        ITagRepository tagRepository) : IGetGeneratedQuestionsByRoom
    {
        public async Task<List<GeneratedQuestionDto>> ExecuteAsync(Guid interviewRoomId, GeneratedQuestionStatus? status)
        {
            var dbTags = await tagRepository.GetAllAsync();
            var tagMap = dbTags.ToDictionary(t => t.Id, t => t.Name);

            var items = status.HasValue
                ? await generatedQuestionRepository.GetByInterviewRoomIdAsync(interviewRoomId, status.Value)
                : await generatedQuestionRepository.GetByInterviewRoomIdAsync(interviewRoomId);

            return items.Select(q => new GeneratedQuestionDto
            {
                Id = q.Id,
                InterviewRoomId = q.InterviewRoomId,
                Title = q.Title,
                Content = q.Content,
                Status = q.Status,
                Tags = q.TagIds
                    .Where(id => tagMap.ContainsKey(id))
                    .Select(id => tagMap[id])
                    .ToList()
            }).ToList();
        }
    }
}
