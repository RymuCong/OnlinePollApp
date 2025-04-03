namespace T3H.Poll.Application.Polls.Services;

public class PollService : CrudService<Domain.Entities.Poll>, IPollService
{
    public PollService(IRepository<Domain.Entities.Poll, Guid> repository, Dispatcher dispatcher) : base(repository, dispatcher)
    {
    }
}