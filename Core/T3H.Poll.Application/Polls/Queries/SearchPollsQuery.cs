using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using T3H.Poll.Application.Common.Queries;
using T3H.Poll.Application.Polls.DTOs;

namespace T3H.Poll.Application.Polls.Queries;

public class SearchPollsQuery : BaseSearchQuery<Domain.Entities.Poll, PollSearchResponse>
{
    public Guid? CreatorId { get; private set; }

    public SearchPollsQuery(SearchPollsQueryParams searchParams)
    {
        SearchRequest = searchParams.ToSearchRequestModel();
        CreatorId = searchParams.CreatorId;
    }

    public override Expression<Func<Domain.Entities.Poll, bool>> GetFilterExpression(SearchCondition condition)
    {
        return condition switch
        {
            // Field search (Title or Description)
            { Field: "FieldSearch", Operator: "contains", Value: var value } when value != null =>
                p => p.Title.Contains((string)value) || p.Description.Contains((string)value),

            // Title search
            { Field: "Title", Operator: "contains", Value: var value } when value != null =>
                p => p.Title.Contains((string)value),

            // Description search
            { Field: "Description", Operator: "contains", Value: var value } when value != null =>
                p => p.Description.Contains((string)value),

            // Creator ID filter
            { Field: "CreatorId", Operator: "eq", Value: var value } when value != null && Guid.TryParse((string)value, out var creatorId) =>
                p => p.CreatorId == creatorId,

            // IsActive filter
            { Field: "IsActive", Operator: "eq", Value: var value } when value != null && bool.TryParse((string)value, out var isActive) =>
                p => p.IsActive == isActive,

            // IsPublic filter
            { Field: "IsPublic", Operator: "eq", Value: var value } when value != null && bool.TryParse((string)value, out var isPublic) =>
                p => p.IsPublic == isPublic,

            // IsAnonymous filter
            { Field: "IsAnonymous", Operator: "eq", Value: var value } when value != null && bool.TryParse((string)value, out var isAnonymous) =>
                p => p.IsAnonymous == isAnonymous,

            // Created date range filters
            { Field: "CreatedDateTime", Operator: "ge", Value: var value } when value != null && DateTime.TryParse((string)value, out var fromDate) =>
                p => p.CreatedDateTime.Date >= fromDate.Date,

            { Field: "CreatedDateTime", Operator: "le", Value: var value } when value != null && DateTime.TryParse((string)value, out var toDate) =>
                p => p.CreatedDateTime.Date <= toDate.Date,

            // StartTime range filters
            { Field: "StartTime", Operator: "ge", Value: var value } when value != null && DateTime.TryParse((string)value, out var fromStart) =>
                p => p.StartTime >= fromStart,

            { Field: "StartTime", Operator: "le", Value: var value } when value != null && DateTime.TryParse((string)value, out var toStart) =>
                p => p.StartTime <= toStart,

            // EndTime range filters
            { Field: "EndTime", Operator: "ge", Value: var value } when value != null && DateTime.TryParse((string)value, out var fromEnd) =>
                p => p.EndTime.HasValue && p.EndTime.Value >= fromEnd,

            { Field: "EndTime", Operator: "le", Value: var value } when value != null && DateTime.TryParse((string)value, out var toEnd) =>
                p => p.EndTime.HasValue && p.EndTime.Value <= toEnd,

            // Default case
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
            EndTime = poll.EndTime ?? DateTime.MinValue
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
            ("updateddatetime", true) => query.OrderByDescending(p => p.UpdatedDateTime),
            ("updateddatetime", false) => query.OrderBy(p => p.UpdatedDateTime),
            ("starttime", true) => query.OrderByDescending(p => p.StartTime),
            ("starttime", false) => query.OrderBy(p => p.StartTime),
            _ => query.OrderByDescending(p => p.CreatedDateTime)
        };
    }
}

public class SearchPollsQueryHandler : BaseSearchQueryHandler<Domain.Entities.Poll, PollSearchResponse, SearchPollsQuery>
{
    public SearchPollsQueryHandler(ICrudService<Domain.Entities.Poll> crudService)
        : base(crudService)
    {
    }

    public override async Task<SearchResponseModel<PollSearchResponse>> HandleAsync(
        SearchPollsQuery query,
        CancellationToken cancellationToken = default)
    {
        // Get base query for polls with all filters applied
        var baseQuery = await PrepareBaseQueryAsync(query, cancellationToken);

        if (baseQuery == null)
        {
            return new SearchResponseModel<PollSearchResponse>
            {
                Items = new List<PollSearchResponse>(),
                TotalItems = 0,
                TotalPages = 0,
                CurrentPage = query.SearchRequest?.PageNumber ?? 1
            };
        }

        // Get total count for pagination metadata
        var totalItems = await baseQuery.CountAsync(cancellationToken);

        // Apply pagination
        var paginatedPolls = await baseQuery
            .Skip((query.SearchRequest.PageNumber - 1) * query.SearchRequest.PageSize)
            .Take(query.SearchRequest.PageSize)
            .ToListAsync(cancellationToken);

        // Format polls with select expression
        var pollDtos = new List<PollSearchResponse>();
        foreach (var poll in paginatedPolls)
        {
            var pollDto = query.GetSelectExpression().Compile()(poll);
            pollDtos.Add(pollDto);
        }

        // Return properly formatted response
        return new SearchResponseModel<PollSearchResponse>
        {
            Items = pollDtos,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)query.SearchRequest.PageSize),
            CurrentPage = query.SearchRequest.PageNumber
        };
    }
}