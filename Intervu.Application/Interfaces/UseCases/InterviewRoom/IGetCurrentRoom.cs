using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.InterviewRoom
{
    public interface IGetCurrentRoom
    {
        Task<Domain.Entities.InterviewRoom> ExecuteAsync(int roomId);
    }
}
