using Intervu.Domain.Entities;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Interviewer
{
    public interface IGetInterviewerDetails
    {
        Task<User> ExecuteAsync(Guid interviewerId);
    }
}
