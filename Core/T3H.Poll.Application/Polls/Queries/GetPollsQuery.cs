using Microsoft.EntityFrameworkCore;
using T3H.Poll.Application.Polls.DTOs;
using T3H.Poll.Infrastructure.Caching;

namespace T3H.Poll.Application.Polls.Queries;

// Redis key constants class
public static class RedisKeyConstants
{
    public const string GetPollsByUserId = "polls:by-user-id";
}

public class GetPagedPollByUserIdQuery : IQuery<Paged<PollResponse>>
{
    public Guid CreatorId { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

internal class GetPagedPollByUserIdQueryHandler : IQueryHandler<GetPagedPollByUserIdQuery, Paged<PollResponse>>
{
    private readonly IPollRepository _pollRepository;
    private readonly IMapper _mapper;
    private readonly RedisCacheService _cacheService;

    public GetPagedPollByUserIdQueryHandler(IPollRepository pollRepository, IMapper mapper, RedisCacheService cacheService)
    {
        _pollRepository = pollRepository;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<Paged<PollResponse>> HandleAsync(GetPagedPollByUserIdQuery request, CancellationToken cancellationToken)
    {
        // Create a unique Redis key based on user ID, page and page size
        var redisKey = $"{RedisKeyConstants.GetPollsByUserId}:{request.CreatorId}:{request.Page}:{request.PageSize}";
        
        // Try to get data from Redis first
        var cachedResult = await _cacheService.GetAsync<Paged<PollResponse>>(redisKey);
        if (cachedResult != null)
        {
            return cachedResult;
        }
        
        // If not in cache, query the database
        var query = _pollRepository.GetQueryableSet()
            .Where(p => p.CreatorId == request.CreatorId);
        
        var totalItems = await query.CountAsync(cancellationToken);
        
        var polls = await query.Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        
        var pollResponses = _mapper.Map<IEnumerable<PollResponse>>(polls).ToList();
        
        var result = new Paged<PollResponse>
        {
            Items = pollResponses,
            TotalItems = totalItems
        };
        
        // Store in Redis for future requests (with optional expiration time)
        await _cacheService.SetAsync(redisKey, result, TimeSpan.FromMinutes(30));
        
        return result;
    }
}