namespace Intervu.Application.Interfaces.UseCases.RescheduleRequest
{
    public interface IRespondToRescheduleRequestUseCase
    {
        Task ExecuteAsync(Guid requestId, Guid respondedBy, bool isApproved, string? rejectionReason);
    }
}
