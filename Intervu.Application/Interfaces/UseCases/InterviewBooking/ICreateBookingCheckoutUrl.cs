namespace Intervu.Application.Interfaces.UseCases.InterviewBooking
{
    public interface ICreateBookingCheckoutUrl
    {
        Task<string> ExecuteAsync(Guid candidateId, Guid coachId, Guid coachAvailabilityId, string returnUrl);
    }
}
