namespace Intervu.Application.Interfaces.UseCases.CandidateProfile
{
    public interface IDeleteCandidateProfile
    {
        Task DeleteCandidateProfileAsync(Guid id);
    }
}
