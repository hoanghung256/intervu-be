using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class AnswerRepository : RepositoryBase<Answer>, IAnswerRepository
    {
        public AnswerRepository(IntervuPostgreDbContext context) : base(context)
        {
        }
    }
}
