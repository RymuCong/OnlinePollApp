using T3H.Poll.Application.Polls.DTOs;

namespace T3H.Poll.Application.Polls.Commands;

public class AddUpdatePollCommand : ICommand
{
    public PollDto PollDto { get; set; }
}

public class AddUpdatePollValidator
{
    public static void Validate(AddUpdatePollCommand request)
    {
        ValidationException.NotNullOrWhiteSpace(request.PollDto.Title, "Tên cuộc khảo sát không được để trống.");
        ValidationException.NotNullOrWhiteSpace(request.PollDto.Description, "Mô tả không được để trống.");
        ValidationException.NotNullOrWhiteSpace(request.PollDto.StartTime.ToString(), "Thời gian bắt đầu không được để trống.");
        ValidationException.NotNullOrWhiteSpace(request.PollDto.EndTime.ToString(), "Thời gian kết thúc không được để trống.");
        ValidationException.NotPastDate(request.PollDto.StartTime, "Thời gian bắt đầu không được là thời gian trong quá khứ.");
    }
}

internal class AddUpdatePollCommandHandler : ICommandHandler<AddUpdatePollCommand, PollDto>
{
    private readonly ICrudService<Domain.Entities.Poll> _pollService;
    private readonly IUnitOfWork _unitOfWork;

    public AddUpdatePollCommandHandler(IUnitOfWork unitOfWork = null,
        ICrudService<Domain.Entities.Poll> pollService = null)
    {
        _unitOfWork = unitOfWork;
        _pollService = pollService;
    }

    public async Task<PollDto> HandleAsync(AddUpdatePollCommand command, CancellationToken cancellationToken = default)
    {
        AddUpdatePollValidator.Validate(command);

        Domain.Entities.Poll poll;
        
        using (await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken))
        {
            if (command.PollDto.Id != Guid.Empty)
            {
                // Update existing poll
                poll = await _pollService.GetByIdAsync(command.PollDto.Id);
                if (poll == null)
                    throw new NotFoundException($"Poll with ID {command.PollDto.Id} not found");
                // Update poll properties
                poll = Domain.Entities.Poll.Create(
                    command.PollDto.Title, 
                    command.PollDto.Description,
                    command.PollDto.StartTime, 
                    command.PollDto.StartTime, 
                    command.PollDto.IsActive,
                    command.PollDto.IsAnonymous,
                    command.PollDto.IsMultipleVotesAllowed, 
                    command.PollDto.IsViewableByModerator, 
                    command.PollDto.IsPublic,
                    command.PollDto.AccessCode, 
                    command.PollDto.VotingFrequencyControl,
                    command.PollDto.VotingCooldownMinutes
                );
                
                await _pollService.UpdateAsync(poll);
            }
            else
            {
                // Create new poll
                poll = Domain.Entities.Poll.Create(
                    command.PollDto.Title, 
                    command.PollDto.Description,
                    command.PollDto.StartTime, 
                    command.PollDto.StartTime, 
                    command.PollDto.IsActive,
                    command.PollDto.IsAnonymous,
                    command.PollDto.IsMultipleVotesAllowed, 
                    command.PollDto.IsViewableByModerator, 
                    command.PollDto.IsPublic,
                    command.PollDto.AccessCode, 
                    command.PollDto.VotingFrequencyControl,
                    command.PollDto.VotingCooldownMinutes
                );
                
                await _pollService.AddAsync(poll);
            }
            
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        
        // Convert to DTO and return
        return new PollDto
        {
            Id = poll.Id,
            Title = poll.Title,
            Description = poll.Description,
            StartTime = poll.StartTime,
            EndTime = poll.EndTime,
            IsActive = poll.IsActive,
            IsAnonymous = poll.IsAnonymous,
            IsMultipleVotesAllowed = poll.IsMultipleVotesAllowed,
            IsViewableByModerator = poll.IsViewableByModerator,
            IsPublic = poll.IsPublic,
            AccessCode = poll.AccessCode,
            VotingFrequencyControl = poll.VotingFrequencyControl,
            VotingCooldownMinutes = poll.VotingCooldownMinutes,
            CreatedDateTime = poll.CreatedDateTime,
            UpdatedDateTime = poll.UpdatedDateTime,
            UserNameUpdated = poll.UserNameUpdated,
            UserNameCreated = poll.UserNameCreated
        };
    }
}