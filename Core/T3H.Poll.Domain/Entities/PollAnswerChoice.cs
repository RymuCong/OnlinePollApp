namespace T3H.Poll.Domain.Entities;

public class PollAnswerChoice : Entity<Guid>
{
    public Guid PollAnswerId { get; set; }
    public Guid ChoiceId { get; set; }
    public int? RankOrder { get; set; } // For ranking questions
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation properties
    public PollAnswer PollAnswer { get; set; } = null!;
    public Choice Choice { get; set; } = null!;
}