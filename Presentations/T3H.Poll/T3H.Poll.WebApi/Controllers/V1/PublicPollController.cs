using Application.Common.Exceptions;
using T3H.Poll.Application.Polls.Commands;
using T3H.Poll.Application.Polls.DTOs;
using T3H.Poll.Application.Polls.Queries;

namespace T3H.Poll.WebApi.Controllers.V1;

[EnableRateLimiting(RateLimiterPolicyNames.DefaultPolicy)]
[AllowAnonymous] // Public APIs don't require authentication
[Produces("application/json")]
[ApiController]
[ApiVersion("1.0")]
[Route("/api/v{version:apiVersion}/poll/")]
public class PublicPollController : ControllerBase
{
    private readonly Dispatcher _dispatcher;

    public PublicPollController(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new CustomException(nameof(dispatcher));
    }

    /// <summary>
    /// Fetch public polls with optional filtering and pagination (polls only, no questions/choices)
    /// </summary>
    /// <param name="searchParams">Search parameters for filtering and pagination</param>
    /// <returns>Paginated list of public polls</returns>
    [HttpGet("public")]
    [ProducesResponseType(typeof(SearchResponseModel<PublicPollDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<SearchResponseModel<PublicPollDto>>> GetPublicPolls(
        [FromQuery] SearchPublicPollsQueryParams searchParams)
    {
        try
        {
            // Validate search parameters
            if (searchParams.PageSize > 50)
            {
                return BadRequest("Page size cannot exceed 50");
            }

            if (searchParams.Page < 1)
            {
                return BadRequest("Page number must be greater than 0");
            }

            var query = new SearchPublicPollsQuery(searchParams);
            var result = await _dispatcher.DispatchAsync(query);

            return Ok(result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(StatusCodes.Status500InternalServerError,
                "An error occurred while fetching public polls");
        }
    }

    /// <summary>
    /// Get a specific public poll by ID with all questions and choices
    /// </summary>
    /// <param name="id">Poll ID</param>
    /// <param name="accessCode">Access code for private polls</param>
    /// <returns>Public poll details</returns>
    [HttpGet("public/{id}")]
    [ProducesResponseType(typeof(PublicPollDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<PublicPollDto>> GetPublicPoll(
        Guid id,
        [FromQuery] string? accessCode = null)
    {
        ValidationException.Requires(id != Guid.Empty, "Invalid poll ID");
    
        var query = new GetPublicPollQuery
        {
            PollId = id,
            AccessCode = accessCode
        };
    
        try
        {
            var result = await _dispatcher.DispatchAsync(query);
            return Ok(result);
        }
        catch (NotFoundException)
        {
            return NotFound($"Poll with ID {id} not found or is not public");
        }
        catch (UnauthorizedException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
        }
    }
    
    /// <summary>
    /// Submit answers for a poll
    /// </summary>
    /// <param name="request">Poll submission request with answers</param>
    /// <returns>Submission confirmation</returns>
    [HttpPost("submit")]
    [ProducesResponseType(typeof(PollSubmissionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PollSubmissionResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<PollSubmissionResponse>> SubmitPollAnswers(
        [FromBody] PollSubmissionRequest request)
    {
        if (request == null)
            return BadRequest("Request body is required");

        ValidationException.Requires(request.PollId != Guid.Empty, "Invalid poll ID");
        ValidationException.Requires(request.Answers != null && request.Answers.Any(), "At least one answer is required");

        var command = new SubmitPollAnswersCommand
        {
            PollId = request.PollId,
            AccessCode = request.AccessCode,
            ParticipantEmail = request.ParticipantEmail,
            ParticipantName = request.ParticipantName,
            Answers = request.Answers
        };

        try
        {
            await _dispatcher.DispatchAsync(command);
            return Ok(new PollSubmissionResponse
            {
                IsSuccessful = true,
                Message = "Poll submitted successfully",
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new PollSubmissionResponse
            {
                IsSuccessful = false,
                Message = "Validation failed",
                ValidationErrors = new List<string> { ex.Message }
            });
        }
    }
}
