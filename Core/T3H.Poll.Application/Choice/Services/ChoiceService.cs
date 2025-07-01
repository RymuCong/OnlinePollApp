namespace T3H.Poll.Application.Choice.Services;

public class ChoiceService : CrudService<Domain.Entities.Choice>, IChoiceService
{
    public ChoiceService(IRepository<Domain.Entities.Choice, Guid> repository, Dispatcher dispatcher) : base(repository, dispatcher)
    {
    }
}