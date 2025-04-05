namespace T3H.Poll.Persistence.Repositories;

public class PollRepository : Repository<Domain.Entities.Poll, Guid>, IPollRepository
{
    public PollRepository(CrmDbContext dbContext, IDateTimeProvider dateTimeProvider) : base(dbContext, dateTimeProvider)
    {
    }
}