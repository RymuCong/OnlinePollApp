using System.ComponentModel.DataAnnotations.Schema;

namespace T3H.Poll.Domain.Entities;

public class PollAnswer : Entity<Guid>, ITrackable
{
    public Guid SubmissionId { get; set; }
    public Guid QuestionId { get; set; }
    public string QuestionType { get; set; } = string.Empty;
    public string? TextAnswer { get; set; }
    public Guid? SingleChoiceId { get; set; } // For single choice, rating, yes/no
    public DateTimeOffset AnsweredAt { get; set; }

    // Navigation properties
    public PollSubmission Submission { get; set; } = null!;
    public Question Question { get; set; } = null!;
    public Choice? SingleChoice { get; set; }
    public ICollection<PollAnswerChoice> SelectedChoices { get; set; } = new List<PollAnswerChoice>();

    // Helper property for backward compatibility
    [NotMapped]
    public List<Guid>? SelectedChoiceIds
    {
        get => SelectedChoices?.Select(sc => sc.ChoiceId).ToList();
        set
        {
            if (value != null)
            {
                SelectedChoices = value.Select((choiceId, index) => new PollAnswerChoice
                {
                    Id = Guid.NewGuid(),
                    PollAnswerId = Id,
                    ChoiceId = choiceId,
                    RankOrder = index + 1,
                    CreatedAt = DateTimeOffset.UtcNow
                }).ToList();
            }
        }
    }
}