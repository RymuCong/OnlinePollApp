using T3H.Poll.Application.Users.Services;

namespace T3H.Poll.Application.Question.Services;

public class QuestionService : CrudService<Domain.Entities.Question>, IQuestionService
{
    public QuestionService(IRepository<Domain.Entities.Question, Guid> repository, Dispatcher dispatcher) : base(repository, dispatcher)
    {
    }
}