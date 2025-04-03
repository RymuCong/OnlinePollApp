namespace T3H.Poll.Application.Polls.DTOs;

public class PollResponse : PollRequest
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedDateTime { get; set; }
    public DateTimeOffset UpdatedDateTime { get; set; }
    public string? UserNameUpdated { get; set; }
    public string? UserNameCreated { get; set; }
}