namespace Intervu.Application.Interfaces.UseCases.IntervieweeProfile
{
    public interface IDeleteIntervieweeProfile
    {
        Task DeleteIntervieweeProfileAsync(Guid id);
    }
}
