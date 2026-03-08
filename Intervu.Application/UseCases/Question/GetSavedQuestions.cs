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

            var snapshotIds = snapshots.Select(s => s.Id).ToList();
            var likedIds = await likeRepository.GetLikedQuestionIdsAsync(userId, snapshotIds);

            return snapshots.Select(s => new QuestionListItemDto
            {
                Id = s.Id,
                Title = s.Title,
                Content = s.Content,
                Level = Enum.Parse<ExperienceLevel>(s.Level),
                Round = Enum.Parse<InterviewRound>(s.Round),
                Status = Enum.Parse<QuestionStatus>(s.Status),
                ViewCount = s.ViewCount,
                SaveCount = s.SaveCount,
                Vote = s.Vote,
                IsHot = s.IsHot,
                CreatedAt = s.CreatedAt,
                CompanyNames = s.CompanyNames,
                Roles = s.Roles,
                Tags = s.Tags.Select(t => new TagDto { Id = Guid.Empty, Name = t }).ToList(),
                Category = s.Category,
                IsLikedByUser = likedIds.Contains(s.Id),
                IsSavedByUser = true
            }).ToList();
        }
    }
}

