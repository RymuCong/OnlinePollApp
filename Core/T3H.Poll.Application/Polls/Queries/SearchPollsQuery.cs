using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using T3H.Poll.Application.Common.Queries;
using T3H.Poll.Application.Polls.DTOs;

namespace T3H.Poll.Application.Polls.Queries;

public class SearchPollsQuery : BaseSearchQuery<Domain.Entities.Poll, PollSearchResponse>
{
    public SearchPollsQuery(SearchPollsQueryParams searchParams)
    {
        SearchRequest = searchParams.ToSearchRequestModel();
    }

    public override Expression<Func<Domain.Entities.Poll, bool>> GetFilterExpression(SearchCondition condition)
    {
        return condition switch
        {
            {Field: "FieldSearch", Operator: "contains"}  =>
                p => p.Title.Contains((string)condition.Value) || p.Description.Contains((string)condition.Value),

            {Field: "CreatorId", Operator: "eq"} =>
                p => p.CreatorId == Guid.Parse((string)condition.Value),

            _ => null
        };
    }

    public override Expression<Func<Domain.Entities.Poll, PollSearchResponse>> GetSelectExpression()
    {
        return poll => new PollSearchResponse
        {
            Id = poll.Id,
            Title = poll.Title,
            Description = poll.Description ?? string.Empty,
            CreatorId = poll.CreatorId,
            IsActive = poll.IsActive,
            UpdatedDateTime = poll.UpdatedDateTime.HasValue ? poll.UpdatedDateTime.Value.DateTime : DateTime.MinValue,
            IsPublic = poll.IsPublic,
            IsAnonymous = poll.IsAnonymous,
            IsMultipleVotesAllowed = poll.IsMultipleVotesAllowed,
            IsViewableByModerator = poll.IsViewableByModerator,
            StartTime = poll.StartTime,
            EndTime = poll.EndTime ?? DateTime.MinValue,
        };
    }

    public override IQueryable<Domain.Entities.Poll> AddIncludes(IQueryable<Domain.Entities.Poll> query)
    {
        return query;
    }

    public override IOrderedQueryable<Domain.Entities.Poll> ApplySort(
        IQueryable<Domain.Entities.Poll> query,
        string sortField,
        bool isDescending)
    {
        return (sortField.ToLower(), isDescending) switch
        {
            ("title", true) => query.OrderByDescending(p => p.Title),
            ("title", false) => query.OrderBy(p => p.Title),
            ("createddatetime", true) => query.OrderByDescending(p => p.CreatedDateTime),
            ("createddatetime", false) => query.OrderBy(p => p.CreatedDateTime),
            _ => query.OrderByDescending(p => p.CreatedDateTime)
        };
    }
}

public class SearchPollsQueryHandler : BaseSearchQueryHandler<Domain.Entities.Poll, PollSearchResponse, SearchPollsQuery>
{
    private readonly IRepository<Domain.Entities.Poll, Guid> _pollRepository;
    private readonly IMapper _mapper;

    public SearchPollsQueryHandler(
        ICrudService<Domain.Entities.Poll> crudService,
        IRepository<Domain.Entities.Poll, Guid> pollRepository,
        IMapper mapper)
        : base(crudService)
    {
        _pollRepository = pollRepository;
        _mapper = mapper;
    }

    public override async Task<ListResultModel<PollSearchResponse>> HandleAsync(
        SearchPollsQuery query,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = _crudService.GetQueryableSet();

        // Apply filters from search conditions
        var conditions = query.SearchRequest?.Conditions;
        if (conditions != null)
        {
            foreach (var condition in conditions)
            {
                var predicate = query.GetFilterExpression(condition);
                if (predicate != null)
                {
                    baseQuery = baseQuery.Where(predicate);
                }
            }
        }

        // Apply sorting
        var sortedQuery = query.ApplySort(baseQuery, query.SearchRequest?.SortField ?? "updateddatetime",
            query.SearchRequest?.IsDescending ?? true);

        // Get total count
        var totalItems = await sortedQuery.CountAsync(cancellationToken);

        // Apply paging
        int page = query.SearchRequest?.PageNumber ?? 1;
        int pageSize = query.SearchRequest?.PageSize ?? 10;

        // Use the select expression defined in the query instead of ProjectTo
        var selectExpression = query.GetSelectExpression();
        var items = await sortedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(selectExpression)
            .ToListAsync(cancellationToken);

        return ListResultModel<PollSearchResponse>.Create(
            items,
            totalItems,
            page,
            pageSize,
            (int)Math.Ceiling(totalItems / (double)pageSize)
        );
    }
}