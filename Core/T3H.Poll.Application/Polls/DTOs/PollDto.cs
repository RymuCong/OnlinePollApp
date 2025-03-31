namespace T3H.Poll.Application.Polls.DTOs;

public class PollDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsActive { get; set; }
    public bool IsAnonymous { get; set; }
    public bool IsMultipleVotesAllowed { get; set; }
    public bool IsViewableByModerator { get; set; }
    public bool IsPublic { get; set; }
    public string AccessCode { get; set; }
    public string VotingFrequencyControl { get; set; }
    public int VotingCooldownMinutes { get; set; }
    public DateTimeOffset CreatedDateTime { get; set; }
    public DateTimeOffset? UpdatedDateTime { get; set; }
    public string UserNameUpdated { get; set; }
    public string UserNameCreated { get; set; }
}