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

            var experience = question.InterviewExperience;
            var author = experience?.User;

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

            // Load related questions (same type or role)
            var related = await questionRepository.GetRelatedAsync(
                questionId,
                question.QuestionType,
                experience?.Role ?? string.Empty,
                RelatedLimit);

            var relatedDtos = related.Select(r => new RelatedQuestionDto
            {
                Id = r.Id,
                Content = r.Content,
                CompanyName = r.InterviewExperience?.CompanyName ?? string.Empty,
                Role = r.InterviewExperience?.Role ?? string.Empty,
                CreatedAt = r.CreatedAt
            }).ToList();

            return new QuestionDetailDto
            {
                Id = question.Id,
                Content = question.Content,
                QuestionType = question.QuestionType,
                CompanyName = experience?.CompanyName ?? string.Empty,
                Role = experience?.Role ?? string.Empty,
                Level = experience?.Level,
                CreatedAt = question.CreatedAt,
                AuthorId = author?.Id ?? Guid.Empty,
                AuthorName = author?.FullName ?? "Anonymous",
                AuthorProfilePicture = author?.ProfilePicture,
                AuthorSlug = author?.SlugProfileUrl ?? string.Empty,
                SaveCount = 0,
                IWasAskedThisCount = 0,
                CommentCount = question.Comments.Count,
                Comments = comments,
                RelatedQuestions = relatedDtos
            };
        }
    }
}
