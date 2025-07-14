using Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using T3H.Poll.Application.Polls.DTOs;
using T3H.Poll.Domain.Enums.EnumExtensions;

namespace T3H.Poll.Application.Polls.Queries;

public class GetPublicPollQuery : IQuery<PublicPollDto>
{
    public Guid PollId { get; set; }
    public string? AccessCode { get; set; }
}

internal class GetPublicPollQueryHandler : IQueryHandler<GetPublicPollQuery, PublicPollDto>
{
    private readonly IPollRepository _pollRepository;

    public GetPublicPollQueryHandler(IPollRepository pollRepository)
    {
        _pollRepository = pollRepository;
    }

    public async Task<PublicPollDto> HandleAsync(GetPublicPollQuery request, CancellationToken cancellationToken)
    {
        var poll = await _pollRepository.GetQueryableSet()
            .Include(p => p.Questions)
                .ThenInclude(q => q.Choices)
            .FirstOrDefaultAsync(p => p.Id == request.PollId, cancellationToken);

        if (poll == null)
        {
            throw new NotFoundException("Poll not found");
        }

        // Check if poll is public or if access code is provided for private polls
        if (!poll.IsPublic)
        {
            if (string.IsNullOrEmpty(request.AccessCode))
            {
                throw new UnauthorizedException("Access code is required for this poll");
            }

            if (!string.IsNullOrEmpty(poll.AccessCode) && poll.AccessCode != request.AccessCode)
            {
                throw new UnauthorizedException("Invalid access code");
            }
        }

        // Map to public DTO
        return MapToPublicPollDto(poll);
    }

    private PublicPollDto MapToPublicPollDto(Domain.Entities.Poll poll)
    {
        var now = DateTime.UtcNow;
        PollStatus status;
        
        if (!poll.IsActive)
            status = PollStatus.Inactive;
        else if (poll.StartTime > now)
            status = PollStatus.NotStarted;
        else if (poll.EndTime < now)
            status = PollStatus.Ended;
        else
            status = PollStatus.Active;

        return new PublicPollDto
        {
            Id = poll.Id,
            Title = poll.Title,
            Description = poll.Description,
            StartTime = poll.StartTime,
            EndTime = poll.EndTime ?? DateTime.MaxValue,
            IsActive = poll.IsActive,
            IsAnonymous = poll.IsAnonymous,
            IsMultipleVotesAllowed = poll.IsMultipleVotesAllowed,
            IsPublic = poll.IsPublic,
            RequiresAccessCode = !string.IsNullOrEmpty(poll.AccessCode),
            VotingFrequencyControl = poll.VotingFrequencyControl,
            VotingCooldownMinutes = poll.VotingCooldownMinutes,
            Status = status,
            Questions = poll.Questions?
                .Where(q => q.IsActive)
                .OrderBy(q => q.QuestionOrder)
                .Select(q => new PublicQuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType.GetDescription(),
                    IsRequired = q.IsRequired,
                    QuestionOrder = q.QuestionOrder,
                    MediaUrl = q.MediaUrl,
                    Settings = q.Settings,
                    Choices = q.Choices?
                        .Where(c => c.IsActive == true)
                        .OrderBy(c => c.ChoiceOrder)
                        .Select(c => new PublicChoiceDto
                        {
                            Id = c.Id,
                            ChoiceText = c.ChoiceText,
                            ChoiceOrder = c.ChoiceOrder,
                            MediaUrl = c.MediaUrl
                        }).ToList() ?? new List<PublicChoiceDto>()
                }).ToList() ?? new List<PublicQuestionDto>()
        };
    }
}
