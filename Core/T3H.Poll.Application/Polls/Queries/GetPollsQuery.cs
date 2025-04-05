using Microsoft.EntityFrameworkCore;
using T3H.Poll.Application.Polls.DTOs;

namespace T3H.Poll.Application.Polls.Queries;

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

    public GetPagedPollByUserIdQueryHandler(IPollRepository pollRepository, IMapper mapper)
    {
        _pollRepository = pollRepository;
        _mapper = mapper;
    }

    public async Task<Paged<PollResponse>> HandleAsync(GetPagedPollByUserIdQuery request, CancellationToken cancellationToken)
    {
        var query = _pollRepository.GetQueryableSet()
            .Where(p => p.CreatorId == request.CreatorId);
        
        var totalItems = await query.CountAsync(cancellationToken);
        
        var polls = await query.Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        
        var pollResponses = _mapper.Map<IEnumerable<PollResponse>>(polls).ToList();
        
        return new Paged<PollResponse>
        {
            Items = pollResponses,
            TotalItems = totalItems
        };
        
    }
}
