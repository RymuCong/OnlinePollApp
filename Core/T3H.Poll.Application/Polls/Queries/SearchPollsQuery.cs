using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using T3H.Poll.Application.Common.Queries;
using T3H.Poll.Application.Polls.DTOs;

namespace T3H.Poll.Application.Polls.Queries;

public class SearchPollsQuery : BaseSearchQuery<Domain.Entities.Poll, PollResponse>
{
    public string Title { get; set; }
    public string Description { get; set; }
    public Guid? CreatorId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public override Expression<Func<Domain.Entities.Poll, bool>> GetFilterExpression(SearchCondition condition)
    {
        return condition switch
        {
            { Field: "title", Operator: "contains" } =>
                p => p.Title.Contains((string)condition.Value),
            
            { Field: "description", Operator: "contains" } =>
                p => p.Description.Contains((string)condition.Value),
            
            { Field: "creatorId", Operator: "eq" } =>
                p => p.CreatorId == (Guid)condition.Value,
                
            { Field: "isActive", Operator: "eq" } =>
                p => p.IsActive == (bool)condition.Value,
                
            { Field: "isPublic", Operator: "eq" } =>
                p => p.IsPublic == (bool)condition.Value,
                
            _ => null
        };
    }

    public override Expression<Func<Domain.Entities.Poll, PollResponse>> GetSelectExpression()
    {
        // return p => new PollResponse
        // {
        //     Id = p.Id,
        //     Title = p.Title,
        //     Description = p.Description,
        //     CreatedDateTime = p.CreatedDateTime,
        //     UserNameCreated = p.UserNameCreated,
        //     IsActive = p.IsActive,
        //     IsPublic = p.IsPublic,
        // };
        return null;
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

public class SearchPollsQueryHandler : BaseSearchQueryHandler<Domain.Entities.Poll, PollResponse, SearchPollsQuery>
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

    public override async Task<ListResultModel<PollResponse>> HandleAsync(
        SearchPollsQuery query,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = _crudService.GetQueryableSet();

        // Apply filters based on provided parameters
        if (!string.IsNullOrWhiteSpace(query.Title))
        {
            baseQuery = baseQuery.Where(p => p.Title.Contains(query.Title));
        }

        if (!string.IsNullOrWhiteSpace(query.Description))
        {
            baseQuery = baseQuery.Where(p => p.Description.Contains(query.Description));
        }

        if (query.CreatorId.HasValue && query.CreatorId != Guid.Empty)
        {
            baseQuery = baseQuery.Where(p => p.CreatorId == query.CreatorId);
        }

        // Apply sorting
        var sortedQuery = query.ApplySort(baseQuery, query.SearchRequest?.SortField ?? "createddatetime", 
            query.SearchRequest?.IsDescending ?? true);

        // Get total count
        var totalItems = await sortedQuery.CountAsync(cancellationToken);

        // Apply paging
        var items = await sortedQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ProjectTo<PollResponse>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return ListResultModel<PollResponse>.Create(
            items,
            totalItems,
            query.Page,
            query.PageSize,
            (int)Math.Ceiling(totalItems / (double)query.PageSize)
        );
    }
}