namespace Intervu.Application.Interfaces.UseCases.RescheduleRequest
{
    public interface ICreateRescheduleRequestUseCase
    {
        Task<Guid> ExecuteAsync(Guid bookingId, Guid proposedAvailabilityId, Guid requestedBy, string reason);
    }
}
