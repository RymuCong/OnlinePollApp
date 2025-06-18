using System.Linq.Expressions;

namespace T3H.Poll.Application.Common.Services;

public interface ICrudService<T> where T : Entity<Guid>, IAggregateRoot
{
    public IQueryable<T> GetQueryableSet();

    Task<List<T>> GetAsync(CancellationToken cancellationToken = default);

    IQueryable<T> GetQueryableSet(CancellationToken cancellationToken = default);

    Task<List<T>> GetByConditionAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddOrUpdateAsync(T entity, CancellationToken cancellationToken = default);

    Task AddRangerAsync(List<T> entities, CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    Task RemoveRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
}
