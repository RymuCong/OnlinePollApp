namespace T3H.Poll.Persistence.Repositories;

public class QuestionRepository : Repository<T3H.Poll.Domain.Entities.Question, Guid>, IQuestionRepository
{
    public QuestionRepository(CrmDbContext dbContext, IDateTimeProvider dateTimeProvider) : base(dbContext, dateTimeProvider)
    {
    }
}