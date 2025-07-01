using T3H.Poll.Domain.Entities;
using T3H.Poll.Infrastructure.Web.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Security.Claims;

namespace T3H.Poll.Application.Common.Services;

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
        if (entity.Id.Equals(default) || await _repository.GetQueryableSet().FirstOrDefaultAsync(p => p.Id == entity.Id) is null)
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
        entity.UserNameCreated = HttpContextCustom.Current?.User?.Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault()?.Value;
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _dispatcher.DispatchAsync(new EntityCreatedEvent<T>(entity, DateTime.UtcNow), cancellationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            entity.UserNameUpdated = HttpContextCustom.Current?.User?.Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault()?.Value;
            await _repository.UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _dispatcher.DispatchAsync(new EntityUpdatedEvent<T>(entity, DateTime.UtcNow), cancellationToken);
        }
        catch (Exception ex)
        {

            throw;
        }
        
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _repository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _dispatcher.DispatchAsync(new EntityDeletedEvent<T>(entity, DateTime.UtcNow), cancellationToken);
    }

    public async Task RemoveRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null || !entities.Any())
        {
            return;
        }

        _repository.RemoveRange(entities);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var entity in entities)
        {
            await _dispatcher.DispatchAsync(new EntityDeletedEvent<T>(entity, DateTime.UtcNow), cancellationToken);
        }
    }
}
