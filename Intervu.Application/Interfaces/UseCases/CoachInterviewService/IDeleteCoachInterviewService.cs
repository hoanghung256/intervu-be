namespace Intervu.Application.Interfaces.UseCases.CoachInterviewService
{
    public interface IDeleteCoachInterviewService
    {
        Task ExecuteAsync(Guid coachId, Guid serviceId);
    }
}
