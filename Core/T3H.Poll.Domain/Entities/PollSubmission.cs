namespace T3H.Poll.Domain.Entities;

public class PollSubmission : Entity<Guid>, IAggregateRoot
{
    public Guid PollId { get; set; }
    public string? ParticipantEmail { get; set; }
    public string? ParticipantName { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    
    // Navigation properties
    public Poll Poll { get; set; } = null!;
    public ICollection<PollAnswer> Answers { get; set; } = new List<PollAnswer>();
}
