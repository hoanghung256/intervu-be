using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class IntervieweeProfileRepository(IntervuPostgreDbContext context) : RepositoryBase<IntervieweeProfile>(context), IIntervieweeProfileRepository
    {
    }
}
