namespace Intervu.Application.Interfaces.UseCases.InterviewBooking
{
    public interface ICreateBookingCheckoutUrl
    {
        Task<string> ExecuteAsync(Guid intervieweeId, Guid interviewerId, Guid interviewerAvailabilityId, string returnUrl);
    }
}
