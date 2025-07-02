using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using T3H.Poll.Application.Common.Queries;
using T3H.Poll.Application.Question.DTOs;
using T3H.Poll.Domain.Enums;

namespace T3H.Poll.Application.Question.Queries;

public class SearchQuestionQuery : BaseSearchQuery<Domain.Entities.Question, QuestionSearchResponse>
{
    public Guid? PollId { get; private set; }
    public Guid? CreatorId { get; private set; }

    public SearchQuestionQuery(SearchQuestionQueryParams queryParams)
    {
        SearchRequest = queryParams.ToSearchRequest();
        PollId = queryParams.PollId;
        CreatorId = queryParams.CreatorId;
    }

    public override Expression<Func<Domain.Entities.Question, bool>> GetFilterExpression(SearchCondition condition)
    {
        return condition switch
        {
            // Question text search
            { Field: "QuestionText", Operator: "contains", Value: var value } when value != null =>
                x => x.QuestionText.Contains((string)value),

            // Question type filter
            { Field: "QuestionType", Operator: "eq", Value: var value } when value != null && Enum.TryParse<QuestionType>((string)value, true, out var questionType) =>
                x => x.QuestionType == questionType,

            // Poll ID filter
            { Field: "PollId", Operator: "eq", Value: var value } when value != null && Guid.TryParse((string)value, out var pollId) =>
                x => x.PollId == pollId,

            // IsRequired filter
            { Field: "IsRequired", Operator: "eq", Value: var value } when value != null && bool.TryParse((string)value, out var isRequired) =>
                x => x.IsRequired == isRequired,

            // IsActive filter
            { Field: "IsActive", Operator: "eq", Value: var value } when value != null && bool.TryParse((string)value, out var isActive) =>
                x => x.IsActive == isActive,

            // Creator filter (through Poll relationship)
            { Field: "CreatorId", Operator: "eq", Value: var value } when value != null && Guid.TryParse((string)value, out var creatorId) =>
                x => x.Poll != null && x.Poll.CreatorId == creatorId,

            // Created date range filters
            { Field: "CreatedDateTime", Operator: "ge", Value: var value } when value != null && DateTime.TryParse((string)value, out var fromDate) =>
                x => x.CreatedDateTime.Date >= fromDate.Date,

            { Field: "CreatedDateTime", Operator: "le", Value: var value } when value != null && DateTime.TryParse((string)value, out var toDate) =>
                x => x.CreatedDateTime.Date <= toDate.Date,

            // Question order range filters
            { Field: "QuestionOrder", Operator: "ge", Value: var value } when value != null && int.TryParse((string)value, out var fromOrder) =>
                x => x.QuestionOrder >= fromOrder,

            { Field: "QuestionOrder", Operator: "le", Value: var value } when value != null && int.TryParse((string)value, out var toOrder) =>
                x => x.QuestionOrder <= toOrder,

            // Default case
            _ => null
        };
    }

    public override Expression<Func<Domain.Entities.Question, QuestionSearchResponse>> GetSelectExpression()
    {
        return question => new QuestionSearchResponse
        {
            Id = question.Id,
            PollId = question.PollId,
            QuestionText = question.QuestionText,
            QuestionType = question.QuestionType.ToString(),
            IsRequired = question.IsRequired,
            QuestionOrder = question.QuestionOrder,
            MediaUrl = question.MediaUrl,
            Settings = question.Settings,
            IsActive = question.IsActive,
            CreatedDateTime = question.CreatedDateTime,
            UpdatedDateTime = question.UpdatedDateTime
        };
    }

    public override IQueryable<Domain.Entities.Question> AddIncludes(IQueryable<Domain.Entities.Question> query)
    {
        return query.Include(q => q.Poll);
    }

    public override IOrderedQueryable<Domain.Entities.Question> ApplySort(
        IQueryable<Domain.Entities.Question> query,
        string sortField,
        bool isDescending)
    {
        return (sortField.ToLower(), isDescending) switch
        {
            ("questionorder", true) => query.OrderByDescending(q => q.QuestionOrder),
            ("questionorder", false) => query.OrderBy(q => q.QuestionOrder),
            ("questiontext", true) => query.OrderByDescending(q => q.QuestionText),
            ("questiontext", false) => query.OrderBy(q => q.QuestionText),
            ("questiontype", true) => query.OrderByDescending(q => q.QuestionType),
            ("questiontype", false) => query.OrderBy(q => q.QuestionType),
            ("createddatetime", true) => query.OrderByDescending(q => q.CreatedDateTime),
            ("createddatetime", false) => query.OrderBy(q => q.CreatedDateTime),
            ("isrequired", true) => query.OrderByDescending(q => q.IsRequired),
            ("isrequired", false) => query.OrderBy(q => q.IsRequired),
            _ => query.OrderBy(q => q.QuestionOrder)
        };
    }
}

public class SearchQuestionQueryHandler : BaseSearchQueryHandler<Domain.Entities.Question, QuestionSearchResponse, SearchQuestionQuery>
{
    private readonly ICrudService<Domain.Entities.Poll> _pollService;

    public SearchQuestionQueryHandler(
        ICrudService<Domain.Entities.Question> crudService,
        ICrudService<Domain.Entities.Poll> pollService)
        : base(crudService)
    {
        _pollService = pollService;
    }

    public override async Task<SearchResponseModel<QuestionSearchResponse>> HandleAsync(
        SearchQuestionQuery query,
        CancellationToken cancellationToken = default)
    {
        // Get base query for questions with all filters applied
        var baseQuery = await PrepareBaseQueryAsync(query, cancellationToken);

        // Apply CreatorId filter through Poll relationship if specified
        if (query.CreatorId.HasValue)
        {
            var pollIds = await _pollService.GetQueryableSet()
                .Where(p => p.CreatorId == query.CreatorId.Value)
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            if (pollIds.Any())
            {
                baseQuery = baseQuery.Where(q => pollIds.Contains(q.PollId));
            }
            else
            {
                // No polls found for this creator, return empty result
                return new SearchResponseModel<QuestionSearchResponse>
                {
                    Items = new List<QuestionSearchResponse>(),
                    TotalItems = 0,
                    TotalPages = 0,
                    CurrentPage = query.SearchRequest?.PageNumber ?? 1
                };
            }
        }

        if (baseQuery == null)
        {
            return new SearchResponseModel<QuestionSearchResponse>
            {
                Items = new List<QuestionSearchResponse>(),
                TotalItems = 0,
                TotalPages = 0,
                CurrentPage = query.SearchRequest?.PageNumber ?? 1
            };
        }

        // Get total count for pagination metadata
        var totalItems = await baseQuery.CountAsync(cancellationToken);

        // Apply pagination
        var paginatedQuestions = await baseQuery
            .Skip((query.SearchRequest.PageNumber - 1) * query.SearchRequest.PageSize)
            .Take(query.SearchRequest.PageSize)
            .ToListAsync(cancellationToken);

        // Format questions with select expression
        var questionDtos = new List<QuestionSearchResponse>();
        foreach (var question in paginatedQuestions)
        {
            var questionDto = query.GetSelectExpression().Compile()(question);
            questionDtos.Add(questionDto);
        }

        // Return properly formatted response
        return new SearchResponseModel<QuestionSearchResponse>
        {
            Items = questionDtos,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)query.SearchRequest.PageSize),
            CurrentPage = query.SearchRequest.PageNumber
        };
    }
}