namespace T3H.Poll.Application.Choice.Services;

public interface IChoiceService : ICrudService<Domain.Entities.Choice>
{
    Task<List<Domain.Entities.Choice>> GetActiveChoicesByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken = default);
    Task<List<Domain.Entities.Choice>> GetChoicesByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken = default);
    Task SoftDeleteChoicesAsync(List<Guid> choiceIds, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, List<Domain.Entities.Choice>>> GetChoicesByQuestionIdsAsync(List<Guid> questionIds, CancellationToken cancellationToken = default);
}