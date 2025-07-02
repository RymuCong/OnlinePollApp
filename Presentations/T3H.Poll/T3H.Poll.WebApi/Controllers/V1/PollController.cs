using Application.Common.Exceptions;
using T3H.Poll.Application.Polls.Commands;
using T3H.Poll.Application.Polls.DTOs;
using T3H.Poll.Application.Polls.Queries;
using T3H.Poll.Application.Question.Commands;
using T3H.Poll.Application.Question.DTOs;
using T3H.Poll.Application.Question.Queries;
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
    
    // Question APIs

    // Search questions in a poll
    [HttpGet("{pollId}/questions/search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<SearchResponseModel<QuestionSearchResponse>>> SearchQuestions(
        Guid pollId,
        [FromQuery] SearchQuestionQueryParams queryParams)
    {
        try
        {
            ValidationException.Requires(pollId != Guid.Empty, "Invalid Poll Id");

            // Set pollId from route parameter
            queryParams.PollId = pollId;

            // Check if user is searching in their own poll or restrict access
            if (queryParams.CreatorId.HasValue && 
                queryParams.CreatorId.Value != _currentUser.UserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    ResultModel<SearchResponseModel<QuestionSearchResponse>>.Create(null, true, 
                    "You can only search questions in polls you created", 403));
            }

            var query = new SearchQuestionQuery(queryParams);
            var result = await _dispatcher.DispatchAsync(query);

            if (!result.Items.Any())
            {
                return NotFound(ResultModel<SearchResponseModel<QuestionSearchResponse>>.Create(
                    result, true, "No questions found matching the search criteria.", 404));
            }

            return Ok(result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ResultModel<SearchResponseModel<QuestionSearchResponse>>.Create(
                null, true, ex.Message, 400));
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ResultModel<SearchResponseModel<QuestionSearchResponse>>.Create(
                    null, true, ex.Message, 403));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                ResultModel<SearchResponseModel<QuestionSearchResponse>>.Create(
                    null, true, $"Error searching questions: {ex.Message}", 500));
        }
    }

    // [HttpGet("questions/{questionId}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [MapToApiVersion("1.0")]
    // public async Task<ActionResult<QuestionDto>> GetQuestionById(Guid questionId)
    // {
    //     ValidationException.Requires(questionId != Guid.Empty, "Invalid Question Id");
    //     
    //     try
    //     {
    //         var question = await _dispatcher.DispatchAsync(new GetQuestionByIdQuery { Id = questionId });
    //         return Ok(question);
    //     }
    //     catch (NotFoundException ex)
    //     {
    //         return NotFound(ResultModel<QuestionDto>.Create(null, true, ex.Message, 404));
    //     }
    //     catch (ForbiddenException ex)
    //     {
    //         return StatusCode(StatusCodes.Status403Forbidden, 
    //             ResultModel<QuestionDto>.Create(null, true, ex.Message, 403));
    //     }
    //     catch (Exception ex)
    //     {
    //         return BadRequest(ResultModel<QuestionDto>.Create(null, true, ex.Message, 400));
    //     }
    // }
    //
    [HttpPost("{pollId}/questions")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<ResultModel<Guid>>> CreateQuestions(Guid pollId, [FromBody] ICollection<QuestionRequest> requests)
    {
        ValidationException.Requires(pollId != Guid.Empty, "Invalid Poll Id");
        ValidationException.Requires(requests != null && requests.Any(), "At least one question must be provided");
    
        try
        {
            await _dispatcher.DispatchAsync(new CreateQuestionCommand(pollId, requests));
            return Ok(ResultModel<Guid>.Create(pollId, false, $"{requests.Count} question(s) created successfully", 201));
        }
        catch (ValidationException ex)
        {
            return BadRequest(ResultModel<Guid>.Create(Guid.Empty, true, ex.Message, 400));
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, 
                ResultModel<Guid>.Create(Guid.Empty, true, ex.Message, 403));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ResultModel<Guid>.Create(Guid.Empty, true, ex.Message, 404));
        }
        catch (Exception ex)
        {
            return BadRequest(ResultModel<Guid>.Create(Guid.Empty, true, ex.Message, 400));
        }
    }
    
    [HttpPut("{pollId}/questions")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<ResultModel<bool>>> UpdateQuestions(Guid pollId, [FromBody] ICollection<QuestionUpdateRequest> requests)
    {
        try
        {
            ValidationException.Requires(pollId != Guid.Empty, "Invalid Poll Id");
            ValidationException.Requires(requests.Any(), "At least one question must be provided");

            await _dispatcher.DispatchAsync(new UpdateQuestionCommand(pollId, requests));
        
            return Ok(ResultModel<bool>.Create(true, false, "Questions updated successfully"));
        }
        catch (ValidationException ex)
        {
            return BadRequest(ResultModel<bool>.Create(false, true, ex.Message, 400));
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, 
                ResultModel<bool>.Create(false, true, ex.Message, 403));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ResultModel<bool>.Create(false, true, ex.Message, 404));
        }
        catch (Exception ex)
        {
            return BadRequest(ResultModel<bool>.Create(false, true, ex.Message, 400));
        }
    }
    
    [HttpDelete("{pollId}/questions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<ResultModel<bool>>> DeleteQuestions(Guid pollId, [FromBody] List<Guid> questionIds)
    {
        try
        {
            ValidationException.Requires(pollId != Guid.Empty, "Invalid Poll Id");
            ValidationException.Requires(questionIds != null && questionIds.Any(), "At least one question ID must be provided");

            await _dispatcher.DispatchAsync(new DeleteQuestionCommand(pollId, questionIds));

            return Ok(ResultModel<bool>.Create(true, false, $"{questionIds.Count} question(s) deleted successfully", 200));
        }
        catch (ValidationException ex)
        {
            return BadRequest(ResultModel<bool>.Create(false, true, ex.Message, 400));
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ResultModel<bool>.Create(false, true, ex.Message, 403));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ResultModel<bool>.Create(false, true, ex.Message, 404));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                ResultModel<bool>.Create(false, true, $"Error deleting questions: {ex.Message}", 500));
        }
    }
}