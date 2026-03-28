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
    public class GetGeneratedQuestionsByRoom(IGeneratedQuestionRepository generatedQuestionRepository) : IGetGeneratedQuestionsByRoom
    {
        public async Task<List<GeneratedQuestionDto>> ExecuteAsync(Guid interviewRoomId, GeneratedQuestionStatus? status)
        {
            var items = status.HasValue
                ? await generatedQuestionRepository.GetByInterviewRoomIdAsync(interviewRoomId, status.Value)
                : await generatedQuestionRepository.GetByInterviewRoomIdAsync(interviewRoomId);

            return items.Select(q => new GeneratedQuestionDto
            {
                Id = q.Id,
                InterviewRoomId = q.InterviewRoomId,
                Title = q.Title,
                Content = q.Content,
                Status = q.Status
            }).ToList();
        }
    }
}
