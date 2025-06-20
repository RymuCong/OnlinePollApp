namespace T3H.Poll.Application.Polls.DTOs;

public class PollRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsActive { get; set; }
    public bool IsAnonymous { get; set; }
    public bool IsMultipleVotesAllowed { get; set; }
    public bool IsViewableByModerator { get; set; }
    public bool IsPublic { get; set; }
    public string? AccessCode { get; set; }
    public string? VotingFrequencyControl { get; set; }
    public int VotingCooldownMinutes { get; set; }
}