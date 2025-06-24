using Application.Common.Exceptions;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using T3H.Poll.Application.Polls.Commands;
using T3H.Poll.Application.Polls.DTOs;
using T3H.Poll.Application.Polls.Queries;
using T3H.Poll.Domain.Identity;

namespace T3H.Poll.WebApi.Controllers.V1;

[EnableRateLimiting(RateLimiterPolicyNames.DefaultPolicy)]
[Authorize]
[Produces("application/json")]
[ApiController]
[ApiVersion("1.0")]
[Route("/api/v{version:apiVersion}/[controller]/")]
public class PollController : ControllerBase
{
    private readonly Dispatcher _dispatcher;
    private readonly ICurrentUser _currentUser;

    public PollController(Dispatcher dispatcher, ICurrentUser currentUser)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    // [Authorize(AuthorizationPolicyNames.GetPollsPolicy)]
    [HttpGet("user/{creatorId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    
    // Search by poll title, description, or creator
    // To do: Fix admin authorization to allow searching all polls
    [HttpGet("search")]    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<ListResultModel<PollResponse>>> SearchPolls(
        [FromQuery] SearchPollsQueryParams queryParams)
    {
        if (queryParams.CreatorId.HasValue && 
            queryParams.CreatorId.Value != _currentUser.UserId)
        {
            return StatusCode(StatusCodes.Status404NotFound, 
                "You can only view polls that you created");
        }
        
        var query = new SearchPollsQuery(queryParams);
        var result = await _dispatcher.DispatchAsync(query);
        
        if (!result.Items.Any())
        {
            return NotFound(ResultModel<ListResultModel<PollResponse>>.Create(null, true, "No polls found matching the search criteria.", 404));
        }
        
        return Ok(result);
    }
    
    
    // [Authorize(AuthorizationPolicyNames.UpdatePollPolicy)]
    [HttpPut("{id}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> Put([FromBody] PollRequest request, Guid id)
    {
        try
        {
            ValidationException.Requires(id != Guid.Empty, "Invalid poll ID");
        
            await _dispatcher.DispatchAsync(new UpdatePollCommand 
            { 
                PollRequest = request, 
                Id = id 
            });
        
            return Ok(ResultModel<PollRequest>.Create(request, false, "Poll updated successfully", 200));
        }
        catch (ValidationException ex)
        {
            return BadRequest(ResultModel<PollRequest>.Create(request, true, ex.Message, 400));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ResultModel<PollRequest>.Create(null, true, ex.Message, 404));
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, 
                ResultModel<PollRequest>.Create(null, true, ex.Message, 403));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ResultModel<PollRequest>.Create(null, true, $"Error updating poll: {ex.Message}", 500));
        }
    }

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