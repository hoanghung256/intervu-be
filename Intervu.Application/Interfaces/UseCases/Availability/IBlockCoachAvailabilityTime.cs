namespace Intervu.Application.Interfaces.UseCases.Availability
{
    public interface IBlockCoachAvailabilityTime
    {
        Task ExecuteAsync(Guid availabilityId, DateTime startTime, DateTime endTime, string? reason);
    }
}
