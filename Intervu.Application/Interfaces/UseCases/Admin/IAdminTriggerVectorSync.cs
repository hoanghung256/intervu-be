using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Admin
{
    public interface IAdminTriggerVectorSync
    {
        /// <summary>Enqueues a full vector sync for the given namespace ("coaches" or "questions").</summary>
        Task ExecuteAsync(string @namespace);
    }
}
