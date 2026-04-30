namespace Intervu.Application.Interfaces.UseCases.InterviewRoom
{
    public interface IOnRoomCompleted
    {
        Task ExecuteAsync(Guid roomId);
    }
}
