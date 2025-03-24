namespace FDS.CRM.Domain.Specification;

public interface ISpecification<T>
{
    IQueryable<T> ApplySpecification(IQueryable<T> query);
    IQueryable<TResult> ApplySpecification<TResult>(IQueryable<T> query, Expression<Func<T, TResult>> selector);
}
