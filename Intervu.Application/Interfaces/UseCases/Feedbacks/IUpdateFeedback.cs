using Intervu.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Feedbacks
{
    public interface IUpdateFeedback
    {
        Task ExecuteAsync(Feedback updatedFeedback);
    }
}
