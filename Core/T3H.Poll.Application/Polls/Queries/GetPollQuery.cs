using T3H.Poll.Application.Polls.DTOs;

namespace T3H.Poll.Application.Polls.Queries;

public class GetPollQuery : IQuery<ResultModel<PollResponse>>
{  
    public Guid Id {get; set; }
}

internal class GetPollQueryHandler : IQueryHandler<GetPollQuery, ResultModel<PollResponse>>
{
    private readonly IPollRepository _pollRepository;
    private readonly IMapper _mapper;

    public GetPollQueryHandler(IPollRepository pollRepository, IMapper mapper)
    {
        _pollRepository = pollRepository;
        _mapper = mapper;
    }

    public async Task<ResultModel<PollResponse>> HandleAsync (GetPollQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var poll = await _pollRepository.SingleOrDefaultAsync(_pollRepository.GetQueryableSet().Where(x => x.Id == request.Id));
            if (poll is null)
            {
                throw new NotFoundException($"Poll {request.Id} not found.");
            }
            var result = _mapper.Map<PollResponse>(poll);
            
            return ResultModel<PollResponse>.Create(result);
        }
        catch (Exception ex)
        {
            return ResultModel<PollResponse>.Create(null, true, "Có lỗi xảy ra khi thao tác với DB", 400);
            //throw ex;
        }
    }
}