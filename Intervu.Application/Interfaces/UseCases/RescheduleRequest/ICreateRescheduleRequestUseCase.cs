namespace Intervu.Application.Interfaces.UseCases.RescheduleRequest
{
    public interface ICreateRescheduleRequestUseCase
    {
        Task<Guid> ExecuteAsync(Guid roomId, DateTime newStartTime, Guid requestedBy, string reason);
    }
}
