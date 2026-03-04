using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Question;
using Intervu.Application.Interfaces.UseCases.Question;
using Intervu.Domain.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Question
{
    public class GetQuestionList(IQuestionRepository repository)
        : IGetQuestionList
    {
        public async Task<PagedResult<QuestionListItemDto>> ExecuteAsync(QuestionFilterRequest filter)
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

            var topAnswer = (Domain.Entities.Answer a) => new AnswerPreviewDto
            {
                Id = a.Id,
                Content = a.Content,
                Upvotes = a.Upvotes,
                IsVerified = a.IsVerified,
                AuthorName = a.Author?.FullName ?? "Anonymous",
                AuthorProfilePicture = a.Author?.ProfilePicture,
                CreatedAt = a.CreatedAt
            };

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
                AnswerCount = q.Answers?.Count ?? 0,
                IsHot = q.IsHot,
                CreatedAt = q.CreatedAt,
                CompanyNames = q.QuestionCompanies?.Select(qc => qc.Company?.Name ?? string.Empty).ToList() ?? new(),
                Roles = q.QuestionRoles?.Select(qr => qr.Role.ToString()).ToList() ?? new(),
                Tags = q.QuestionTags?.Select(qt => new TagDto { Id = qt.TagId, Name = qt.Tag?.Name ?? string.Empty }).ToList() ?? new(),
                Category = q.Category.ToString(),
                TopAnswer = q.Answers?.OrderByDescending(a => a.IsVerified).ThenByDescending(a => a.Upvotes).Select(a => topAnswer(a)).FirstOrDefault()
            }).ToList();

            return new PagedResult<QuestionListItemDto>(dtos, total, filter.PageSize, filter.Page);
        }
    }
}
