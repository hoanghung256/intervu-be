using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Domain.Repositories
{
    public interface IRepositoryUnitOfWork : IDisposable
    {
        Task<int> CommitAsync();
    }
}
