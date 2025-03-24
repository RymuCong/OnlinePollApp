using System.Linq.Expressions;

namespace FDS.CRM.Application.Common.Services;

public class CrudService<T> : ICrudService<T> where T : Entity<Guid>, IAggregateRoot
{
    private readonly IUnitOfWork _unitOfWork;
    protected readonly IRepository<T, Guid> _repository;
    protected readonly Dispatcher _dispatcher;

    public CrudService(IRepository<T, Guid> repository, Dispatcher dispatcher)
    {
        _unitOfWork = repository.UnitOfWork;
        _repository = repository;
        _dispatcher = dispatcher;
    }
    public IQueryable<T> GetQueryableSet()
    {
        return _repository.GetQueryableSet();
    }
    public Task<List<T>> GetAsync(CancellationToken cancellationToken = default)
    {
        return _repository.ToListAsync(_repository.GetQueryableSet());
    }
    public IQueryable<T> GetQueryableSet(CancellationToken cancellationToken = default)
    {
        return _repository.GetQueryableSet();
    }

    public Task<List<T>> GetByConditionAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return _repository.ToListAsync(_repository.GetQueryableSet().Where(predicate));
    }

    public Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ValidationException.Requires(id != Guid.Empty, "Invalid Id");
        return _repository.FirstOrDefaultAsync(_repository.GetQueryableSet().Where(x => x.Id == id));
    }

    public async Task AddRangerAsync(List<T> entities, CancellationToken cancellationToken = default)
    {
        await _repository.AddRangerAsync(entities, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AddOrUpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity.Id.Equals(default))
        {
            await AddAsync(entity, cancellationToken);
        }
        else
        {
            await UpdateAsync(entity, cancellationToken);
        }
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _dispatcher.DispatchAsync(new EntityCreatedEvent<T>(entity, DateTime.UtcNow), cancellationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _dispatcher.DispatchAsync(new EntityUpdatedEvent<T>(entity, DateTime.UtcNow), cancellationToken);
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _repository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _dispatcher.DispatchAsync(new EntityDeletedEvent<T>(entity, DateTime.UtcNow), cancellationToken);
    }
}
