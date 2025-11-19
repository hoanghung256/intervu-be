namespace Intervu.Application.Interfaces.UseCases.InterviewBooking
{
    public interface ICreateBookingCheckoutUrl
    {
        Task<string> ExecuteAsync(int intervieweeId, int interviewerId, int interviewerAvailabilityId);
    }
}
