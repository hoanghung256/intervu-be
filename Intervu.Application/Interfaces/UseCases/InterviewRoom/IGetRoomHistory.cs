using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.Interfaces.UseCases.InterviewRoom
{
    public interface IGetRoomHistory
    {
        Task<IEnumerable<Domain.Entities.InterviewRoom>> ExecuteAsync(UserRole role, int userId);
    }
}
