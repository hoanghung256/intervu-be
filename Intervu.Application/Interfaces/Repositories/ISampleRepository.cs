using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.Repositories
{
    public interface ISampleRepository
    {
        Task<IEnumerable<string>> GetSampleDataAsync();
    }
}
