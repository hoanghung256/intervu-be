using Intervu.Application.Interfaces.UseCases.Feedbacks;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Feedbacks
{
    public class CreateFeedback : ICreateFeedback
    {
        private readonly IFeedbackRepository _repo;

        public CreateFeedback(IFeedbackRepository repo)
        {
            _repo = repo;
        }

        public async Task ExecuteAsync(Feedback feedback)
        {
            await _repo.CreateFeedbackAsync(feedback);
        }
    }
}
