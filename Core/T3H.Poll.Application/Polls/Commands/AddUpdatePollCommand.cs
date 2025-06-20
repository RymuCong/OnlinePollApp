using T3H.Poll.Application.Polls.DTOs;
using T3H.Poll.Domain.Identity;
using T3H.Poll.Infrastructure.Caching;

namespace T3H.Poll.Application.Polls.Commands;

public class AddUpdatePollCommand : ICommand
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

internal class AddUpdatePollCommandHandler : ICommandHandler<AddUpdatePollCommand>
{
    private readonly ICrudService<Domain.Entities.Poll> _pollService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly RedisCacheService _cacheService;
 
    public AddUpdatePollCommandHandler(
        IUnitOfWork unitOfWork,
        ICrudService<Domain.Entities.Poll> pollService,
        ICurrentUser currentUser,
        RedisCacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _pollService = pollService;
        _currentUser = currentUser;
        _cacheService = cacheService;
    }

    public async Task HandleAsync(AddUpdatePollCommand command, CancellationToken cancellationToken = default)
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
        
        // delete cache
        var redisKey = $"{RedisKeyConstants.GetPollsByUserId}:{_currentUser.UserId}";
        await _cacheService.RemoveAsync(redisKey);
    }
    
}