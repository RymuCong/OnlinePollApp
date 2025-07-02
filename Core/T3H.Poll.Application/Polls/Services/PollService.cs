using Microsoft.EntityFrameworkCore;

namespace T3H.Poll.Application.Polls.Services;

public class PollService : CrudService<Domain.Entities.Poll>, IPollService
{
    public PollService(IRepository<Domain.Entities.Poll, Guid> repository, Dispatcher dispatcher) : base(repository, dispatcher)
    {
    }
    
    public async Task<List<Domain.Entities.Poll>> GetPollsByIdsAsync(List<Guid> pollIds, CancellationToken cancellationToken = default)
    {
        return await GetQueryableSet()
            .Where(p => pollIds.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }
    
    public async Task SoftDeleteAsync(Guid pollId, CancellationToken cancellationToken = default)
    {
        var poll = await GetByIdAsync(pollId, cancellationToken);
        if (poll != null)
        {
            poll.IsDeleted = true;
            poll.UpdatedDateTime = DateTimeOffset.UtcNow;
            poll.UserNameUpdated = "System";
            await UpdateAsync(poll, cancellationToken);
        }
    }
    
}