using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using T3H.Poll.Application.Common.Queries;
using T3H.Poll.Application.Polls.DTOs;

namespace T3H.Poll.Application.Polls.Queries;

public class SearchPublicPollsQuery : BaseSearchQuery<Domain.Entities.Poll, PublicPollDto>
{
    public SearchPublicPollsQuery(SearchPublicPollsQueryParams searchParams)
    {
        SearchRequest = searchParams.ToSearchRequestModel();
    }

    public override Expression<Func<Domain.Entities.Poll, bool>> GetFilterExpression(SearchCondition condition)
    {
        return condition switch
        {
            // Field search (Title or Description)
            { Field: "FieldSearch", Operator: "contains", Value: var value } when value != null =>
                p => p.Title.Contains((string)value) || p.Description.Contains((string)value),

            // Creator ID filter
            { Field: "CreatorId", Operator: "eq", Value: var value } when value != null && Guid.TryParse((string)value, out var creatorId) =>
                p => p.CreatorId == creatorId,

            // Default case
            _ => null
        };
    }

    public override Expression<Func<Domain.Entities.Poll, PublicPollDto>> GetSelectExpression()
    {
        return poll => new PublicPollDto
        {
            Id = poll.Id,
            Title = poll.Title,
            Description = poll.Description,
            StartTime = poll.StartTime,
            EndTime = poll.EndTime ?? DateTime.MaxValue,
            IsActive = poll.IsActive,
            IsAnonymous = poll.IsAnonymous,
            IsMultipleVotesAllowed = poll.IsMultipleVotesAllowed,
            IsPublic = poll.IsPublic,
            RequiresAccessCode = !string.IsNullOrEmpty(poll.AccessCode),
            VotingFrequencyControl = poll.VotingFrequencyControl,
            VotingCooldownMinutes = poll.VotingCooldownMinutes,
            Status = !poll.IsActive ? PollStatus.Inactive :
                     poll.StartTime > DateTime.UtcNow ? PollStatus.NotStarted :
                     poll.EndTime.HasValue && poll.EndTime < DateTime.UtcNow ? PollStatus.Ended :
                     PollStatus.Active
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
            ("starttime", true) => query.OrderByDescending(p => p.StartTime),
            ("starttime", false) => query.OrderBy(p => p.StartTime),
            ("endtime", true) => query.OrderByDescending(p => p.EndTime),
            ("endtime", false) => query.OrderBy(p => p.EndTime),
            ("createddatetime", true) => query.OrderByDescending(p => p.CreatedDateTime),
            ("createddatetime", false) => query.OrderBy(p => p.CreatedDateTime),
            ("updateddatetime", true) => query.OrderByDescending(p => p.UpdatedDateTime),
            ("updateddatetime", false) => query.OrderBy(p => p.UpdatedDateTime),
            _ => query.OrderByDescending(p => p.UpdatedDateTime)
        };
    }
}

public class SearchPublicPollsQueryHandler : BaseSearchQueryHandler<Domain.Entities.Poll, PublicPollDto, SearchPublicPollsQuery>
{
    public SearchPublicPollsQueryHandler(ICrudService<Domain.Entities.Poll> crudService)
        : base(crudService)
    {
    }

    public override async Task<SearchResponseModel<PublicPollDto>> HandleAsync(
        SearchPublicPollsQuery query,
        CancellationToken cancellationToken = default)
    {
        // Get base query for public polls only
        var baseQuery = await PrepareBaseQueryAsync(query, cancellationToken);

        if (baseQuery == null)
        {
            return new SearchResponseModel<PublicPollDto>
            {
                Items = new List<PublicPollDto>(),
                TotalItems = 0,
                TotalPages = 0,
                CurrentPage = query.SearchRequest?.PageNumber ?? 1
            };
        }

        // Add filter for public polls only
        baseQuery = baseQuery.Where(p => p.IsPublic && p.IsActive);

        // Get total count for pagination metadata
        var totalItems = await baseQuery.CountAsync(cancellationToken);

        // Apply pagination
        var paginatedPolls = await baseQuery
            .Skip((query.SearchRequest.PageNumber - 1) * query.SearchRequest.PageSize)
            .Take(query.SearchRequest.PageSize)
            .ToListAsync(cancellationToken);

        // Format polls with select expression
        var pollDtos = new List<PublicPollDto>();
        foreach (var poll in paginatedPolls)
        {
            var pollDto = query.GetSelectExpression().Compile()(poll);
            pollDtos.Add(pollDto);
        }

        // Return properly formatted response
        return new SearchResponseModel<PublicPollDto>
        {
            Items = pollDtos,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)query.SearchRequest.PageSize),
            CurrentPage = query.SearchRequest.PageNumber
        };
    }
}