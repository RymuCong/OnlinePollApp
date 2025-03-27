namespace T3H.Poll.Domain.Specification;

public abstract class BaseSpecification<T> : ISpecification<T>
{
    protected virtual IQueryable<T> AddIncludes(IQueryable<T> query) => query;
    protected virtual IQueryable<T> AddFilters(IQueryable<T> query) => query;
    protected virtual IQueryable<T> AddSorting(IQueryable<T> query) => query;

    public virtual IQueryable<T> ApplySpecification(IQueryable<T> query)
    {
        query = AddIncludes(query);
        query = AddFilters(query);
        query = AddSorting(query);
        return query;
    }

    public virtual IQueryable<TResult> ApplySpecification<TResult>(
        IQueryable<T> query,
        Expression<Func<T, TResult>> selector)
    {
        return ApplySpecification(query).Select(selector);
    }
}
