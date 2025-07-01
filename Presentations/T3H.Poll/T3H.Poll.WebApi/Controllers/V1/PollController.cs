using Application.Common.Exceptions;
using T3H.Poll.Application.Polls.Commands;
using T3H.Poll.Application.Polls.DTOs;
using T3H.Poll.Application.Polls.Queries;
using T3H.Poll.Application.Question.Commands;
using T3H.Poll.Application.Question.DTOs;
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
    // [HttpGet("{pollId}/questions")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [MapToApiVersion("1.0")]
    // public async Task<ActionResult<List<QuestionDto>>> GetQuestionsByPollId(Guid pollId)
    // {
    //     ValidationException.Requires(pollId != Guid.Empty, "Invalid Poll Id");
    //     
    //     try
    //     {
    //         var questions = await _dispatcher.DispatchAsync(new GetQuestionsByPollIdQuery { PollId = pollId });
    //         return Ok(questions);
    //     }
    //     catch (NotFoundException ex)
    //     {
    //         return NotFound(ResultModel<List<QuestionDto>>.Create(null, true, ex.Message, 404));
    //     }
    //     catch (ForbiddenException ex)
    //     {
    //         return StatusCode(StatusCodes.Status403Forbidden, 
    //             ResultModel<List<QuestionDto>>.Create(null, true, ex.Message, 403));
    //     }
    //     catch (Exception ex)
    //     {
    //         return BadRequest(ResultModel<List<QuestionDto>>.Create(null, true, ex.Message, 400));
    //     }
    // }
    
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
    public async Task<ActionResult<ResultModel<Guid>>> CreateQuestion(Guid pollId, [FromBody] QuestionRequest request)
    {
        ValidationException.Requires(pollId != Guid.Empty, "Invalid Poll Id");
        
        try
        {
            await _dispatcher.DispatchAsync(new CreateQuestionCommand {PollId = pollId, QuestionRequest = request});
            // return Created($"/api/v1/Poll/questions/", ResultModel<Guid>.Create(pollId, false, "Question created successfully", 201));
            return Ok(ResultModel<Guid>.Create(pollId, false, "Question created successfully", 201));
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
    //
    // [HttpPut("questions/{questionId}")]
    // [Consumes("application/json")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [MapToApiVersion("1.0")]
    // public async Task<ActionResult<ResultModel<bool>>> UpdateQuestion(Guid questionId, [FromBody] QuestionRequest request)
    // {
    //     ValidationException.Requires(questionId != Guid.Empty, "Invalid Question Id");
    //     
    //     try
    //     {
    //         var command = new UpdateQuestionCommand
    //         {
    //             Id = questionId,
    //             QuestionText = request.QuestionText,
    //             QuestionType = request.QuestionType,
    //             IsRequired = request.IsRequired,
    //             QuestionOrder = request.QuestionOrder,
    //             MediaUrl = request.MediaUrl,
    //             Settings = request.Settings,
    //             Choices = request.Choices?.Select(c => new UpdateChoiceModel
    //             {
    //                 Id = c.Id,
    //                 ChoiceText = c.ChoiceText,
    //                 ChoiceOrder = c.ChoiceOrder,
    //                 IsCorrect = c.IsCorrect,
    //                 MediaUrl = c.MediaUrl,
    //                 IsActive = c.IsActive
    //             }).ToList() ?? new List<UpdateChoiceModel>()
    //         };
    //         
    //         var result = await _dispatcher.DispatchAsync(command);
    //         return Ok(ResultModel<bool>.Create(result));
    //     }
    //     catch (ValidationException ex)
    //     {
    //         return BadRequest(ResultModel<bool>.Create(false, true, ex.Message, 400));
    //     }
    //     catch (ForbiddenException ex)
    //     {
    //         return StatusCode(StatusCodes.Status403Forbidden, 
    //             ResultModel<bool>.Create(false, true, ex.Message, 403));
    //     }
    //     catch (NotFoundException ex)
    //     {
    //         return NotFound(ResultModel<bool>.Create(false, true, ex.Message, 404));
    //     }
    //     catch (Exception ex)
    //     {
    //         return BadRequest(ResultModel<bool>.Create(false, true, ex.Message, 400));
    //     }
    // }
    //
    // [HttpDelete("questions/{questionId}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [MapToApiVersion("1.0")]
    // public async Task<ActionResult<ResultModel<bool>>> DeleteQuestion(Guid questionId)
    // {
    //     ValidationException.Requires(questionId != Guid.Empty, "Invalid Question Id");
    //     
    //     try
    //     {
    //         var result = await _dispatcher.DispatchAsync(new DeleteQuestionCommand { Id = questionId });
    //         return Ok(ResultModel<bool>.Create(result));
    //     }
    //     catch (ForbiddenException ex)
    //     {
    //         return StatusCode(StatusCodes.Status403Forbidden, 
    //             ResultModel<bool>.Create(false, true, ex.Message, 403));
    //     }
    //     catch (NotFoundException ex)
    //     {
    //         return NotFound(ResultModel<bool>.Create(false, true, ex.Message, 404));
    //     }
    //     catch (Exception ex)
    //     {
    //         return BadRequest(ResultModel<bool>.Create(false, true, ex.Message, 400));
    //     }
    // }
}