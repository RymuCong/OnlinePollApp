namespace T3H.Poll.Application.Polls.Services;

public interface IPollService : ICrudService<Domain.Entities.Poll>
{
    Task<List<Domain.Entities.Poll>> GetPollsByIdsAsync(List<Guid> pollIds, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Guid pollId, CancellationToken cancellationToken = default);
}