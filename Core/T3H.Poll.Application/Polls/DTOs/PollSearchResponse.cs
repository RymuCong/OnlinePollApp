namespace T3H.Poll.Application.Polls.DTOs;

public class PollSearchResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CreatorId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsActive { get; set; }
    public bool IsAnonymous { get; set; }
    public bool IsMultipleVotesAllowed { get; set; }
    public bool IsViewableByModerator { get; set; }
    public bool IsPublic { get; set; }
    public DateTime UpdatedDateTime { get; set; }
}