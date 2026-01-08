namespace Intervu.Application.Interfaces.UseCases.InterviewBooking
{
    public interface ICreateBookingCheckoutUrl
    {
        Task<string> ExecuteAsync(Guid candidateId, Guid interviewerId, Guid interviewerAvailabilityId, string returnUrl);
    }
}
