using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Question;
using Intervu.Application.Interfaces.UseCases.Question;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Question
{
    public class GetQuestionList(
        IQuestionRepository repository,
        IUserQuestionLikeRepository likeRepository,
        IUserRepository userRepository,
        ICandidateProfileRepository candidateProfileRepository,
        ICoachProfileRepository coachProfileRepository)
        : IGetQuestionList
    {
        public async Task<PagedResult<QuestionListItemDto>> ExecuteAsync(QuestionFilterRequest filter, Guid? userId = null)
        {
            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize < 1) filter.PageSize = 10;

            var (items, total) = await repository.GetPagedAsync(
                filter.SearchTerm,
                filter.CompanyId,
                filter.TagId,
                filter.Category,
                filter.Role,
                filter.Level,
                filter.Round,
                filter.SortBy,
                filter.Page,
                filter.PageSize);

            var questionIds = items.Select(q => q.Id).ToList();

            var likedIds = userId.HasValue
                ? await likeRepository.GetLikedQuestionIdsAsync(userId.Value, questionIds)
                : new HashSet<Guid>();

            HashSet<Guid>? savedIds = null;
            if (userId.HasValue)
            {
                var user = await userRepository.GetByIdAsync(userId.Value);
                if (user != null)
                {
                    if (user.Role == UserRole.Candidate)
                    {
                        var profile = await candidateProfileRepository.GetProfileByIdAsync(userId.Value);
                        savedIds = profile?.SavedQuestions?.Select(s => s.Id).ToHashSet();
                    }
                    else
                    {
                        var profile = await coachProfileRepository.GetProfileByIdAsync(userId.Value);
                        savedIds = profile?.SavedQuestions?.Select(s => s.Id).ToHashSet();
                    }
                }
            }

            var dtos = items.Select(q => new QuestionListItemDto
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
                IsSavedByUser = savedIds?.Contains(q.Id) ?? false
            }).ToList();

            return new PagedResult<QuestionListItemDto>(dtos, total, filter.PageSize, filter.Page);
        }
    }
}

