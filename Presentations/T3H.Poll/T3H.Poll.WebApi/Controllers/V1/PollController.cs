using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using T3H.Poll.Application.Polls.Commands;
using T3H.Poll.Application.Polls.DTOs;
using T3H.Poll.Application.Polls.Queries;

namespace T3H.Poll.WebApi.Controllers.V1;

[EnableRateLimiting(RateLimiterPolicyNames.DefaultPolicy)]
// [Authorize]
[Produces("application/json")]
[ApiController]
[ApiVersion("1.0")]
[Route("/api/v{version:apiVersion}/[controller]/")]
public class PollController : ControllerBase
{
    private readonly Dispatcher _dispatcher;

    public PollController(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    // [Authorize(AuthorizationPolicyNames.GetPollsPolicy)]
    [HttpGet("user/{creatorId}")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<IEnumerable<Paged<PollResponse>>>> GetPollsByUserId(Guid creatorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        ValidationException.Requires(creatorId != Guid.Empty, "Invalid Id");
        
        var polls = await _dispatcher.DispatchAsync(new GetPagedPollByUserIdQuery { CreatorId = creatorId, Page = page, PageSize = pageSize });
        // var model = polls.ToModels();
        return Ok(polls);
    }
    
    // [Authorize(AuthorizationPolicyNames.GetPollPolicy)]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<PollResponse>> Get(Guid id)
    {
        ValidationException.Requires(id != Guid.Empty, "Invalid Id");
        
        var poll = await _dispatcher.DispatchAsync(new GetPollQuery { Id = id});
        // var model = poll.ToModel();
        return Ok(poll);
    }

    // [Authorize(AuthorizationPolicyNames.AddPollPolicy)]
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<ResultModel<PollRequest>>> Post([FromBody] PollRequest request)
    {
        try
        {
            await _dispatcher.DispatchAsync(new AddUpdatePollCommand { PollRequest = request });
            // return Created($"/api/v1/Poll/{dto.Id}", ResultModel<PollDto>.Create(dto));
            return Ok(request);
        }
        catch (Exception ex)
        {
            return BadRequest(ResultModel<PollRequest>.Create(request, true, ex.Message, 400));
        }
    }

    // [Authorize(AuthorizationPolicyNames.UpdatePollPolicy)]
    // [HttpPut("{id}")]
    // [Consumes("application/json")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [MapToApiVersion("1.0")]
    // public async Task<ActionResult> Put(Guid id, [FromBody] PollDto model)
    // {
    //     var poll = await _dispatcher.DispatchAsync(new GetPollQuery { Id = id });
    //
    //     poll.UpdatePoll(
    //         model.Title,
    //         model.Description,
    //         model.EndTime,
    //         model.IsActive,
    //         model.IsAnonymous,
    //         model.IsMultipleVotesAllowed,
    //         model.IsViewableByModerator,
    //         model.IsPublic,
    //         model.AccessCode,
    //         model.ThemeSettings,
    //         model.VotingFrequencyControl,
    //         model.VotingCooldownMinutes,
    //         User.Identity.Name ?? "system"
    //     );
    //
    //     await _dispatcher.DispatchAsync(new AddUpdatePollCommand { Poll = poll });
    //
    //     model = poll.ToModel();
    //     return Ok(model);
    // }

    // [Authorize(AuthorizationPolicyNames.DeletePollPolicy)]
    // [HttpDelete("{id}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [MapToApiVersion("1.0")]
    // public async Task<ActionResult> Delete(Guid id)
    // {
    //     var poll = await _dispatcher.DispatchAsync(new GetPollQuery { Id = id });
    //     await _dispatcher.DispatchAsync(new DeletePollCommand { Poll = poll });
    //
    //     return Ok();
    // }
}