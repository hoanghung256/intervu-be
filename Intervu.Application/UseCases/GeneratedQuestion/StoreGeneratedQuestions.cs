using Intervu.Application.DTOs.Ai;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.GeneratedQuestion;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Application.UseCases.InterviewRoom;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.GeneratedQuestion
{
    public class StoreGeneratedQuestions(
        IGeneratedQuestionRepository generatedQuestionRepository,
        IInterviewRoomRepository interviewRoomRepository,
        IBackgroundService backgroundService) : IStoreGeneratedQuestions
    {
        public async Task<int> ExecuteAsync(Guid interviewRoomId, IEnumerable<AiQuestionDto> questions, string? transcript = null)
        {
            var questionList = questions.ToList();
            var items = questionList
                .Where(q => !string.IsNullOrWhiteSpace(q.Title) || !string.IsNullOrWhiteSpace(q.Content))
                .Select(q => new Domain.Entities.GeneratedQuestion
                {
                    Id = Guid.NewGuid(),
                    InterviewRoomId = interviewRoomId,
                    Title = q.Title?.Trim() ?? string.Empty,
                    Content = q.Content?.Trim() ?? string.Empty,
                    Status = GeneratedQuestionStatus.PendingReview
                })
                .ToList();

            foreach (var item in items)
            {
                await generatedQuestionRepository.AddAsync(item);
            }

            if (items.Count > 0)
            {
                await generatedQuestionRepository.SaveChangesAsync();
            }

            var room = await interviewRoomRepository.GetByIdAsync(interviewRoomId);
            if (room != null)
            {
                room.Transcript = transcript;
                room.QuestionList = questionList.Select(q => new QuestionItem
                {
                    Title = q.Title?.Trim() ?? string.Empty,
                    Content = q.Content?.Trim() ?? string.Empty
                }).ToList();
                interviewRoomRepository.UpdateAsync(room);
                await interviewRoomRepository.SaveChangesAsync();
            }

            if (room?.CoachId != null)
            {
                backgroundService.Enqueue<INotificationUseCase>(
                        uc => uc.CreateAsync(
                            room.CoachId.Value,
                            NotificationType.AiAnalysisCompleted,
                            "Extract successful",
                            "Your interview session questions has been extracted successfully.",
                            $"/interview?roomId={interviewRoomId}&action=review-questions",
                            null));
            }

            return items.Count;
        }
    }
}
