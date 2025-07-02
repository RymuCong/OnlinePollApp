namespace T3H.Poll.Application.Choice.Services;

public interface IChoiceService : ICrudService<Domain.Entities.Choice>
{
    Task<List<Domain.Entities.Choice>> GetActiveChoicesByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken = default);
}