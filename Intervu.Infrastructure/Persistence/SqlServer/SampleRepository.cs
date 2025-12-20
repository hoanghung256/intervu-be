using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.SqlServer.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.Persistence.SqlServer
{
    public class SampleRepository : RepositoryBase<User>, ISampleRepository
    {
        public SampleRepository(IntervuDbContext context) : base(context)
        { }

        public Task<IEnumerable<string>> GetSampleDataAsync()
        {
            throw new NotImplementedException();
        }
    }
}
