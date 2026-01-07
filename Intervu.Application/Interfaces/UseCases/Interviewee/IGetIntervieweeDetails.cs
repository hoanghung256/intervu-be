using Intervu.Domain.Entities;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Interviewee
{
    public interface IGetIntervieweeDetails
    {
        Task<User> ExecuteAsync(Guid intervieweeId);
    }
}
