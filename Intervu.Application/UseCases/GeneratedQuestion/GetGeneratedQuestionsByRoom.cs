using Intervu.Application.DTOs.GeneratedQuestion;
using Intervu.Application.Exceptions;
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
        ITagRepository tagRepository,
        IInterviewRoomRepository interviewRoomRepository) : IGetGeneratedQuestionsByRoom
    {
        public async Task<List<GeneratedQuestionDto>> ExecuteAsync(Guid interviewRoomId, Guid userId, GeneratedQuestionStatus? status)
        {
            var room = await interviewRoomRepository.GetByIdAsync(interviewRoomId)
                ?? throw new NotFoundException("Interview room not found");

            //var isParticipant = (room.CandidateId.HasValue && room.CandidateId.Value == userId)
            //    || (room.CoachId.HasValue && room.CoachId.Value == userId);

            //if (!isParticipant)
            //    throw new ForbiddenException("You are not allowed to access this room");

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
