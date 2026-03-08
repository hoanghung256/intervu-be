using Intervu.Application.DTOs.Comment;
using Intervu.Application.DTOs.Question;
using Intervu.Application.Interfaces.UseCases.Question;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Question
{
    public class GetQuestionDetail(
        IQuestionRepository questionRepository,
        IUserRepository userRepository,
        IUserQuestionLikeRepository questionLikeRepository,
        IUserCommentLikeRepository commentLikeRepository,
        ICandidateProfileRepository candidateProfileRepository,
        ICoachProfileRepository coachProfileRepository)
        : IGetQuestionDetail
    {
        private const int RelatedLimit = 5;

        public async Task<QuestionDetailDto?> ExecuteAsync(Guid questionId, Guid? userId = null)
        {
            var question = await questionRepository.GetDetailAsync(questionId);
            if (question == null) return null;

            await questionRepository.IncrementViewCountAsync(questionId);

            var author = question.Author;

            // Load comment authors
            var commentAuthorIds = question.Comments
                .Select(c => c.CreateBy)
                .Distinct()
                .ToList();

            var authorMap = new Dictionary<Guid, User>();
            foreach (var uid in commentAuthorIds)
            {
                var u = await userRepository.GetByIdAsync(uid);
                if (u != null) authorMap[uid] = u;
            }

            // Resolve liked comment IDs for current user
            var commentIds = question.Comments.Select(c => c.Id).ToList();
            var likedCommentIds = userId.HasValue
                ? await commentLikeRepository.GetLikedCommentIdsAsync(userId.Value, commentIds)
                : new HashSet<Guid>();

            var comments = question.Comments
                .OrderByDescending(c => c.IsAnswer)
                .ThenByDescending(c => c.Vote)
                .ThenBy(c => c.CreatedAt)
                .Select(c =>
                {
                    authorMap.TryGetValue(c.CreateBy, out var commentAuthor);
                    return new CommentDetailDto
                    {
                        Id = c.Id,
                        Content = c.Content,
                        Vote = c.Vote,
                        IsAnswer = c.IsAnswer,
                        CreatedAt = c.CreatedAt,
                        CreatedBy = c.CreateBy,
                        AuthorName = commentAuthor?.FullName ?? "Anonymous",
                        AuthorProfilePicture = commentAuthor?.ProfilePicture,
                        IsLikedByUser = likedCommentIds.Contains(c.Id)
                    };
                }).ToList();

            // Resolve is-liked-by-user for Question
            bool isLikedByUser = false;
            bool isSavedByUser = false;

            if (userId.HasValue)
            {
                isLikedByUser = await questionLikeRepository.HasLikedAsync(userId.Value, questionId);

                var user = await userRepository.GetByIdAsync(userId.Value);
                if (user != null)
                {
                    if (user.Role == UserRole.Candidate)
                    {
                        var profile = await candidateProfileRepository.GetProfileByIdAsync(userId.Value);
                        isSavedByUser = profile?.SavedQuestions?.Any(s => s.Id == questionId) ?? false;
                    }
                    else
                    {
                        var profile = await coachProfileRepository.GetProfileByIdAsync(userId.Value);
                        isSavedByUser = profile?.SavedQuestions?.Any(s => s.Id == questionId) ?? false;
                    }
                }
            }

            // Related questions (same tags or roles)
            var related = await questionRepository.GetRelatedAsync(
                questionId,
                question.Id,
                RelatedLimit);

            var relatedDtos = related.Select(r => new RelatedQuestionDto
            {
                Id = r.Id,
                Title = r.Title,
                CompanyNames = r.QuestionCompanies?.Select(qc => qc.Company?.Name ?? string.Empty).ToList() ?? new(),
                Roles = r.QuestionRoles?.Select(qr => qr.Role.ToString()).ToList() ?? new(),
                CommentCount = r.Comments?.Count ?? 0,
                CreatedAt = r.CreatedAt
            }).ToList();

            return new QuestionDetailDto
            {
                Id = question.Id,
                Title = question.Title,
                Content = question.Content,
                Level = question.Level,
                Round = question.Round,
                Status = question.Status,
                ViewCount = question.ViewCount + 1,
                SaveCount = question.SaveCount,
                Vote = question.Vote,
                CommentCount = question.Comments?.Count ?? 0,
                IsHot = question.IsHot,
                CreatedAt = question.CreatedAt,
                AuthorId = author?.Id,
                AuthorName = author?.FullName ?? "Anonymous",
                AuthorProfilePicture = author?.ProfilePicture,
                AuthorSlug = author?.SlugProfileUrl ?? string.Empty,
                CompanyNames = question.QuestionCompanies?.Select(qc => qc.Company?.Name ?? string.Empty).ToList() ?? new(),
                Roles = question.QuestionRoles?.Select(qr => qr.Role.ToString()).ToList() ?? new(),
                Tags = question.QuestionTags?.Select(qt => new TagDto { Id = qt.TagId, Name = qt.Tag?.Name ?? string.Empty }).ToList() ?? new(),
                Category = question.Category.ToString(),
                Comments = comments,
                RelatedQuestions = relatedDtos,
                IsLikedByUser = isLikedByUser,
                IsSavedByUser = isSavedByUser
            };
        }
    }
}


