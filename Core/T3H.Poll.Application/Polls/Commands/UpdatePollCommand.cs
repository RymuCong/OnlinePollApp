using Application.Common.Exceptions;
using T3H.Poll.Application.Polls.DTOs;
using T3H.Poll.Domain.Identity;
using T3H.Poll.Infrastructure.Caching;

namespace T3H.Poll.Application.Polls.Commands;

public class UpdatePollCommand : ICommand
{
    public Guid Id { get; set; }
    public PollRequest PollRequest { get; set; }
}

public class UpdatePollValidator
{
    public static void Validate(UpdatePollCommand command)
    {
        ValidationException.Requires(command.Id != Guid.Empty, "Poll ID cannot be empty.");
        ValidationException.NotNullOrWhiteSpace(command.PollRequest.Title, "Poll title cannot be empty.");
        ValidationException.NotNullOrWhiteSpace(command.PollRequest.Description, "Poll description cannot be empty.");
        ValidationException.NotNullOrWhiteSpace(command.PollRequest.StartTime.ToString(), "Start time cannot be empty.");
        ValidationException.NotNullOrWhiteSpace(command.PollRequest.EndTime.ToString(), "End time cannot be empty.");
    }
}

internal class UpdatePollCommandHandler : ICommandHandler<UpdatePollCommand>
{
    private readonly ICrudService<Domain.Entities.Poll> _pollService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly RedisCacheService _cacheService;

    public UpdatePollCommandHandler(
        ICrudService<Domain.Entities.Poll> pollService,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        RedisCacheService cacheService)
    {
        _pollService = pollService;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cacheService = cacheService;
    }

    public async Task HandleAsync(UpdatePollCommand command, CancellationToken cancellationToken = default)
    {
        UpdatePollValidator.Validate(command);

        using (await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken))
        {
            var existingPoll = await _pollService.GetByIdAsync(command.Id, cancellationToken);

            if (existingPoll == null)
            {
                throw new NotFoundException($"Poll with ID {command.Id} not found");
            }

            // Authorization check - only allow the creator to update their polls
            if (existingPoll.CreatorId != _currentUser.UserId)
            {
                throw new ForbiddenException("You can only update polls that you created");
            }

            // Update poll properties
            existingPoll.Title = command.PollRequest.Title;
            existingPoll.Description = command.PollRequest.Description;
            existingPoll.StartTime = command.PollRequest.StartTime;
            existingPoll.EndTime = command.PollRequest.EndTime;
            existingPoll.IsActive = command.PollRequest.IsActive;
            existingPoll.IsAnonymous = command.PollRequest.IsAnonymous;
            existingPoll.IsMultipleVotesAllowed = command.PollRequest.IsMultipleVotesAllowed;
            existingPoll.IsViewableByModerator = command.PollRequest.IsViewableByModerator;
            existingPoll.IsPublic = command.PollRequest.IsPublic;
            existingPoll.AccessCode = string.IsNullOrEmpty(command.PollRequest.AccessCode)
                ? null
                : command.PollRequest.AccessCode;
            existingPoll.VotingFrequencyControl = string.IsNullOrEmpty(command.PollRequest.VotingFrequencyControl)
                ? null
                : command.PollRequest.VotingFrequencyControl;
            existingPoll.VotingCooldownMinutes = command.PollRequest.VotingCooldownMinutes;

            await _pollService.UpdateAsync(existingPoll, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Clear cache for the user's polls
            var redisKey = $"{RedisKeyConstants.GetPollsByUserId}:{_currentUser.UserId}";
            await _cacheService.RemoveAsync(redisKey);
        }
    }
}