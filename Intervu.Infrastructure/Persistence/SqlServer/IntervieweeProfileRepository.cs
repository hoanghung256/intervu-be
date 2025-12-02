using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.SqlServer.DataContext;

namespace Intervu.Infrastructure.Persistence.SqlServer
{
    public class IntervieweeProfileRepository : RepositoryBase<IntervieweeProfile>, IIntervieweeProfileRepository
    {
        public IntervieweeProfileRepository(IntervuDbContext context) : base(context)
        {
        }
    }
}
