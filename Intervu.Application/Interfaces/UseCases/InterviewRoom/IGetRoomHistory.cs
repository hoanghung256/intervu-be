using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.InterviewRoom;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.Interfaces.UseCases.InterviewRoom
{
    public interface IGetRoomHistory
    {
        Task<IEnumerable<Domain.Entities.InterviewRoom>> ExecuteAsync(UserRole role, Guid userId);
        Task<IEnumerable<Domain.Entities.InterviewRoom>> ExecuteAsync();
        
        /// <summary>
        /// Get paginated interview rooms with reschedule status
        /// </summary>
        Task<PagedResult<InterviewRoomDto>> ExecuteWithPaginationAsync(
            UserRole role, 
            Guid userId, 
            GetInterviewRoomsRequestDto request);
    }
}
