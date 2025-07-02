namespace T3H.Poll.Application.Question.Services;

public interface IQuestionService : ICrudService<Domain.Entities.Question>
{
    Task<List<Domain.Entities.Question>> GetQuestionsWithChoicesByIdsAsync(List<Guid> questionIds, CancellationToken cancellationToken = default);
    Task SoftDeleteQuestionsAsync(List<Domain.Entities.Question> questions, CancellationToken cancellationToken = default);
    Task<List<Domain.Entities.Question>> GetQuestionsByIdsAsync(List<Guid> questionIds, CancellationToken cancellationToken = default);
}