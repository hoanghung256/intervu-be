using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.CoachProfile
{
    public interface IDeleteCoachProfile
    {
        Task ExecuteAsync(Guid id);
    }
}
