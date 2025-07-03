using Microsoft.EntityFrameworkCore;

namespace T3H.Poll.Application.Choice.Services;

public class ChoiceService : CrudService<Domain.Entities.Choice>, IChoiceService
{
    private readonly IRepository<Domain.Entities.Choice, Guid> _choiceRepository;
    
    public ChoiceService(IRepository<Domain.Entities.Choice, Guid> repository, IRepository<Domain.Entities.Choice, Guid> choiceRepository ,Dispatcher dispatcher) : base(repository, dispatcher)
    {
        _choiceRepository = choiceRepository;
    }
    
    public async Task<List<Domain.Entities.Choice>> GetActiveChoicesByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken = default)
    {
        return await GetQueryableSet()
            .Where(c => c.QuestionId == questionId && c.IsActive == true)
            .OrderBy(c => c.ChoiceOrder)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<List<Domain.Entities.Choice>> GetChoicesByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken = default)
    {
        return await GetQueryableSet()
            .Where(c => c.QuestionId == questionId && c.IsDeleted != true)
            .ToListAsync(cancellationToken);
    }

    public async Task SoftDeleteChoicesAsync(List<Guid> choiceIds, CancellationToken cancellationToken = default)
    {
        var choices = await GetQueryableSet()
            .Where(c => choiceIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        foreach (var choice in choices)
        {
            choice.IsDeleted = true;
            choice.UpdatedDateTime = DateTimeOffset.UtcNow;
        
            await UpdateAsync(choice, cancellationToken);
        }
    }

    public async Task HardDeleteChoicesAsync(List<Guid> choiceIds, CancellationToken cancellationToken = default)
    {
        var choices = await GetQueryableSet()
            .Where(c => choiceIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        foreach (var choice in choices)
        {
            _choiceRepository.Delete(choice);
        }
        
        // Save changes to actually execute the deletions
        await _choiceRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<Dictionary<Guid, List<Domain.Entities.Choice>>> GetChoicesByQuestionIdsAsync(List<Guid> questionIds, CancellationToken cancellationToken = default)
    {
        if (!questionIds.Any())
            return new Dictionary<Guid, List<Domain.Entities.Choice>>();

        var allChoices = await GetQueryableSet()
            .Where(c => questionIds.Contains(c.QuestionId) && c.IsDeleted != true)
            .OrderBy(c => c.ChoiceOrder)
            .ToListAsync(cancellationToken);

        return allChoices
            .GroupBy(c => c.QuestionId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}