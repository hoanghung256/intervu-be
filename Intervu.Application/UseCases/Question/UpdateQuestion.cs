using Intervu.Application.DTOs.Question;
using Intervu.Application.Interfaces.UseCases.Question;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Question
{
    public class UpdateQuestion(IUnitOfWork unitOfWork) : IUpdateQuestion
    {
        public async Task ExecuteAsync(Guid questionId, UpdateQuestionRequest request, Guid userId)
        {
            var questionRepo = unitOfWork.GetRepository<IQuestionRepository>();
            var question = await questionRepo.GetDetailAsync(questionId)
                ?? throw new Exception("Question not found");

            question.Title = request.Title;
            question.Content = request.Content;
            question.Level = request.Level;
            question.Round = request.Round;
            question.Category = request.Category;
            question.UpdatedAt = DateTime.UtcNow;

            question.QuestionCompanies.Clear();
            foreach (var cid in request.CompanyIds)
                question.QuestionCompanies.Add(new QuestionCompany { QuestionId = questionId, CompanyId = cid });

            question.QuestionRoles.Clear();
            foreach (var r in request.Roles)
                question.QuestionRoles.Add(new QuestionRole { QuestionId = questionId, Role = r });

            question.QuestionTags.Clear();
            foreach (var tid in request.TagIds)
                question.QuestionTags.Add(new QuestionTag { QuestionId = questionId, TagId = tid });

            questionRepo.UpdateAsync(question);
            await unitOfWork.SaveChangesAsync();
        }
    }
}
