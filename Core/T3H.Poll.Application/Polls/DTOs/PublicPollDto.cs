namespace T3H.Poll.Application.Polls.DTOs;

public class PublicPollDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsActive { get; set; }
    public bool IsAnonymous { get; set; }
    public bool IsMultipleVotesAllowed { get; set; }
    public bool IsPublic { get; set; }
    public bool RequiresAccessCode { get; set; }
    public string? VotingFrequencyControl { get; set; }
    public int VotingCooldownMinutes { get; set; }
    public List<PublicQuestionDto> Questions { get; set; } = new List<PublicQuestionDto>();
    public PollStatus Status { get; set; }
}

public class PublicQuestionDto
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int QuestionOrder { get; set; }
    public string? MediaUrl { get; set; }
    public string? Settings { get; set; }
    public List<PublicChoiceDto> Choices { get; set; } = new List<PublicChoiceDto>();
}

public class PublicChoiceDto
{
    public Guid Id { get; set; }
    public string ChoiceText { get; set; } = string.Empty;
    public int? ChoiceOrder { get; set; }
    public string? MediaUrl { get; set; }
}

public enum PollStatus
{
    NotStarted,
    Active,
    Ended,
    Inactive
}
