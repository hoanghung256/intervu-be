namespace Intervu.Application.Interfaces.UseCases.Admin
{
    public interface IDeleteUserForAdmin
    {
        Task<bool> ExecuteAsync(Guid userId);
    }
}
