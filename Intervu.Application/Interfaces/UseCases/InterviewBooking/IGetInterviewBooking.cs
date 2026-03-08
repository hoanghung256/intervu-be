using Intervu.Domain.Entities.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.InterviewBooking
{
    public interface IGetInterviewBooking
    {
        Task<Domain.Entities.InterviewBookingTransaction?> GetById(Guid id);

        Task<Domain.Entities.InterviewBookingTransaction?> Get(int orderCode, TransactionType type);
    }
}
