namespace T3H.Poll.Application.Queries;

public interface IQueryHandler<TQuery, TResult>
  where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
