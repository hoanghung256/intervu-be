using Intervu.Application.DTOs.Question;
using Intervu.Application.Interfaces.UseCases.Question;
using Intervu.Domain.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Question
{
    public class SearchQuestions(IQuestionRepository questionRepository) : ISearchQuestions
    {
        public async Task<List<QuestionSearchResultDto>> ExecuteAsync(string keyword, int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<QuestionSearchResultDto>();

            var questions = await questionRepository.SearchAsync(keyword, limit);

            return questions.Select(q => new QuestionSearchResultDto
            {
                Id = q.Id,
                Title = q.Title,
                Content = q.Content,
                CompanyNames = q.QuestionCompanies?.Select(qc => qc.Company?.Name ?? string.Empty).ToList() ?? new(),
                Roles = q.QuestionRoles?.Select(qr => qr.Role.ToString()).ToList() ?? new(),
                Tags = q.QuestionTags?.Select(qt => qt.Tag?.Name ?? string.Empty).ToList() ?? new(),
                AnswerCount = q.Answers?.Count ?? 0
            }).ToList();
        }
    }
}
