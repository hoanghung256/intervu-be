namespace Intervu.Application.Interfaces.UseCases.InterviewBooking
{
    public interface ICancelInterview
    {
        Task<int> ExecuteAsync(Guid interviewRoomId);
    }
}
