namespace T3H.Poll.Persistence.Repositories;

public class ChoiceRepository : Repository<Choice, Guid>, IChoiceRepository
{
    public ChoiceRepository(CrmDbContext dbContext, IDateTimeProvider dateTimeProvider) : base(dbContext, dateTimeProvider)
    {
    }
}