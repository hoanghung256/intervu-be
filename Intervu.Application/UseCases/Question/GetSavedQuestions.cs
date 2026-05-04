using Intervu.Application.DTOs.Question;
using Intervu.Application.Interfaces.UseCases.Question;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Question
{
    public class GetSavedQuestions(
        IUserRepository userRepository,
        ICandidateProfileRepository candidateProfileRepository,
        ICoachProfileRepository coachProfileRepository,
        IQuestionRepository questionRepository,
        IUserQuestionLikeRepository likeRepository) : IGetSavedQuestions
    {
        public async Task<List<QuestionListItemDto>> ExecuteAsync(Guid userId)
        {
            var user = await userRepository.GetByIdAsync(userId)
                ?? throw new Exception("User not found");

            List<QuestionSnapshot>? snapshots = null;

            if (user.Role == UserRole.Candidate)
            {
                var profile = await candidateProfileRepository.GetProfileByIdAsync(userId);
                snapshots = profile?.SavedQuestions;
            }
            else
            {
                var profile = await coachProfileRepository.GetProfileByIdAsync(userId);
                snapshots = profile?.SavedQuestions;
            }

            if (snapshots == null || !snapshots.Any())
                return new List<QuestionListItemDto>();

            var savedIds = snapshots.Select(s => s.Id).ToList();
            var questions = await questionRepository.GetByIdsAsync(savedIds);
            var questionMap = questions.ToDictionary(q => q.Id);
            var likedIds = await likeRepository.GetLikedQuestionIdsAsync(userId, savedIds);

            return savedIds
                .Where(questionMap.ContainsKey) // preserve saved order, skip missing questions
                .Select(id => questionMap[id])
                .Select(q => new QuestionListItemDto
                {
                    Id = q.Id,
                    Title = q.Title,
                    Content = q.Content,
                    Level = q.Level,
                    Round = q.Round,
                    Status = q.Status,
                    ViewCount = q.ViewCount,
                    SaveCount = q.SaveCount,
                    CommentCount = q.Comments?.Count ?? 0,
                    Vote = q.Vote,
                    IsHot = q.IsHot,
                    CreatedAt = q.CreatedAt,
                    CompanyNames = q.QuestionCompanies?.Select(qc => qc.Company?.Name ?? string.Empty).ToList() ?? new(),
                    Roles = q.QuestionRoles?.Select(qr => qr.Role.ToString()).ToList() ?? new(),
                    Tags = q.QuestionTags?.Select(qt => new TagDto { Id = qt.TagId, Name = qt.Tag?.Name ?? string.Empty }).ToList() ?? new(),
                    Category = q.Category.ToString(),
                    IsLikedByUser = likedIds.Contains(q.Id),
                    IsSavedByUser = true
                })
                .ToList();
        }
    }
}

