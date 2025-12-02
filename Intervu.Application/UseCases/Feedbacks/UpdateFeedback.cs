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
    public class UpdateFeedback : IUpdateFeedback
    {
        private readonly IFeedbackRepository _repo;

        public UpdateFeedback(IFeedbackRepository repo)
        {
            _repo = repo;
        }

        public async Task ExecuteAsync(Feedback updatedFeedback)
        {
            await _repo.UpdateFeedbackAsync(updatedFeedback);
        }
    }
}
