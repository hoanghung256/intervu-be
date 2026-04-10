using Intervu.Application.DTOs.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Admin
{
    public interface IGetNeedsAttentionQueue
    {
        Task<List<NeedsAttentionItemDto>> ExecuteAsync();
    }
}
