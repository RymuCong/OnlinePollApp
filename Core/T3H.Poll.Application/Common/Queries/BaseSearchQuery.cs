using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace T3H.Poll.Application.Common.Queries;

public abstract class BaseSearchQuery<TEntity, TResult> : IQuery<ListResultModel<TResult>>
     where TEntity : Entity<Guid>, IAggregateRoot
{
    public SearchRequestModel SearchRequest { get; set; }

    public abstract Expression<Func<TEntity, bool>> GetFilterExpression(SearchCondition condition);
    public abstract Expression<Func<TEntity, TResult>> GetSelectExpression();
    public abstract IQueryable<TEntity> AddIncludes(IQueryable<TEntity> query);
    public abstract IOrderedQueryable<TEntity> ApplySort(IQueryable<TEntity> query, string sortField, bool isDescending);
}

public abstract class BaseSearchQueryHandler<TEntity, TResult, TQuery> : IQueryHandler<TQuery, ListResultModel<TResult>>
    where TEntity : Entity<Guid>, IAggregateRoot
    where TQuery : BaseSearchQuery<TEntity, TResult>
{
    protected readonly ICrudService<TEntity> _crudService;

    protected BaseSearchQueryHandler(ICrudService<TEntity> crudService)
    {
        _crudService = crudService;
    }

    protected virtual async Task<IQueryable<TEntity>> PrepareBaseQueryAsync(
        TQuery query,
        CancellationToken cancellationToken)
    {
        var baseQuery = _crudService.GetQueryableSet();

        // Add includes
        baseQuery = query.AddIncludes(baseQuery);

        // Apply filters
        foreach (var condition in query.SearchRequest.Conditions)
        {
            var filterExpression = query.GetFilterExpression(condition);
            if (filterExpression != null)
            {
                baseQuery = baseQuery.Where(filterExpression);
            }
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(query.SearchRequest.SortField))
        {
            baseQuery = query.ApplySort(baseQuery, query.SearchRequest.SortField, query.SearchRequest.IsDescending);
        }

        return baseQuery;
    }

    public virtual async Task<ListResultModel<TResult>> HandleAsync(
        TQuery request,
        CancellationToken cancellationToken = default)
    {
        var query = await PrepareBaseQueryAsync(request, cancellationToken);

        // Get total count
        var totalItems = await query.CountAsync(cancellationToken);

        // Apply projection and paging
        var selectExpression = request.GetSelectExpression();
        var items = await query
            .Select(selectExpression)
            .Skip((request.SearchRequest.PageNumber - 1) * request.SearchRequest.PageSize)
            .Take(request.SearchRequest.PageSize)
            .ToListAsync(cancellationToken);

        return ListResultModel<TResult>.Create(items, totalItems, request.SearchRequest.PageNumber, request.SearchRequest.PageSize,
            (int)Math.Ceiling(totalItems / (double)request.SearchRequest.PageSize));
    }
}
