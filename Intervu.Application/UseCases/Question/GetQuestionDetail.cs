using Intervu.Application.DTOs.Comment;
using Intervu.Application.DTOs.Question;
using Intervu.Application.Interfaces.UseCases.Question;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Question
{
    public class GetQuestionDetail(IQuestionRepository questionRepository, IUserRepository userRepository)
        : IGetQuestionDetail
    {
        private const int RelatedLimit = 5;

        public async Task<QuestionDetailDto?> ExecuteAsync(Guid questionId)
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
                        AuthorProfilePicture = commentAuthor?.ProfilePicture
                    };
                }).ToList();

            var answers = question.Answers
                .OrderByDescending(a => a.IsVerified)
                .ThenByDescending(a => a.Upvotes)
                .Select(a => new AnswerDetailDto
                {
                    Id = a.Id,
                    Content = a.Content,
                    Upvotes = a.Upvotes,
                    IsVerified = a.IsVerified,
                    AuthorId = a.AuthorId,
                    AuthorName = a.Author?.FullName ?? "Anonymous",
                    AuthorProfilePicture = a.Author?.ProfilePicture,
                    AuthorSlug = a.Author?.SlugProfileUrl ?? string.Empty,
                    CreatedAt = a.CreatedAt
                }).ToList();

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
                AnswerCount = r.Answers?.Count ?? 0,
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
                ViewCount = question.ViewCount + 1, // reflect just-incremented
                SaveCount = question.SaveCount,
                AnswerCount = question.Answers?.Count ?? 0,
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
                Answers = answers,
                Comments = comments,
                RelatedQuestions = relatedDtos
            };
        }
    }
}
