using Microsoft.EntityFrameworkCore;

namespace T3H.Poll.Application.Choice.Services;

public class ChoiceService : CrudService<Domain.Entities.Choice>, IChoiceService
{
    public ChoiceService(IRepository<Domain.Entities.Choice, Guid> repository, Dispatcher dispatcher) : base(repository, dispatcher)
    {
    }
    
    public async Task<List<Domain.Entities.Choice>> GetActiveChoicesByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken = default)
    {
        return await GetQueryableSet()
            .Where(c => c.QuestionId == questionId && c.IsActive == true)
            .OrderBy(c => c.ChoiceOrder)
            .ToListAsync(cancellationToken);
    }
}