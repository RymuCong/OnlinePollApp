using T3H.Poll.Application.Polls.DTOs;
using T3H.Poll.Domain.Identity;
using T3H.Poll.Infrastructure.Caching;

namespace T3H.Poll.Application.Polls.Commands;

public class AddUpdatePollCommand : ICommand<PollResponse>
{
    public PollRequest PollRequest { get; set; }
}

public class AddUpdatePollValidator
{
    public static void Validate(AddUpdatePollCommand request)
    {
        ValidationException.NotNullOrWhiteSpace(request.PollRequest.Title, "Tên cuộc khảo sát không được để trống.");
        ValidationException.NotNullOrWhiteSpace(request.PollRequest.Description, "Mô tả không được để trống.");
        ValidationException.NotNullOrWhiteSpace(request.PollRequest.StartTime.ToString(), "Thời gian bắt đầu không được để trống.");
        ValidationException.NotNullOrWhiteSpace(request.PollRequest.EndTime.ToString(), "Thời gian kết thúc không được để trống.");
        ValidationException.NotPastDate(request.PollRequest.StartTime, "Thời gian bắt đầu không được là thời gian trong quá khứ.");
    }
}

internal class AddUpdatePollCommandHandler : ICommandHandler<AddUpdatePollCommand, PollResponse>
{
    private readonly ICrudService<Domain.Entities.Poll> _pollService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly RedisCacheService _cacheService;

    public AddUpdatePollCommandHandler(
        IUnitOfWork unitOfWork,
        ICrudService<Domain.Entities.Poll> pollService,
        ICurrentUser currentUser
        )
    {
        _unitOfWork = unitOfWork;
        _pollService = pollService;
        _currentUser = currentUser;
    }

    public async Task<PollResponse> HandleAsync(AddUpdatePollCommand command, CancellationToken cancellationToken = default)
    {
        AddUpdatePollValidator.Validate(command);
        Domain.Entities.Poll poll;

        using (await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken))
        {
            poll = Domain.Entities.Poll.Create(
                command.PollRequest.Title,
                command.PollRequest.Description,
                _currentUser.UserId,
                command.PollRequest.StartTime,
                command.PollRequest.EndTime,
                command.PollRequest.IsActive,
                command.PollRequest.IsAnonymous,
                command.PollRequest.IsMultipleVotesAllowed,
                command.PollRequest.IsViewableByModerator,
                command.PollRequest.IsPublic,
                string.IsNullOrEmpty(command.PollRequest.AccessCode) ? null : command.PollRequest.AccessCode,
                string.IsNullOrEmpty(command.PollRequest.VotingFrequencyControl) ? null : command.PollRequest.VotingFrequencyControl,
                command.PollRequest.VotingCooldownMinutes);

            await _pollService.AddAsync(poll);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }

        // Convert to PollResponse and return
        return new PollResponse
        {
            Id = poll.Id,
            Title = poll.Title,
            Description = poll.Description,
            CreatorId = poll.CreatorId,
            StartTime = poll.StartTime,
            EndTime = poll.EndTime.Value,
            IsActive = poll.IsActive,
            IsAnonymous = poll.IsAnonymous,
            IsMultipleVotesAllowed = poll.IsMultipleVotesAllowed,
            IsViewableByModerator = poll.IsViewableByModerator,
            IsPublic = poll.IsPublic,
            AccessCode = poll.AccessCode,
            VotingFrequencyControl = poll.VotingFrequencyControl,
            VotingCooldownMinutes = poll.VotingCooldownMinutes,
            CreatedDateTime = poll.CreatedDateTime,
            UpdatedDateTime = poll.UpdatedDateTime.Value
        };
    }
}