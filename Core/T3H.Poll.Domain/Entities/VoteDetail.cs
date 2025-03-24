using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FDS.CRM.Domain.Entities;

public class VoteDetail : Entity<Guid>
{
    [Required]
    public Guid QuestionId { get; set; }

    public Guid UserId { get; set; }  // Nullable for anonymous voting

    public Guid ChoiceId { get; set; }  // Nullable for text answers

    public string TextAnswer { get; set; }

    [StringLength(50)]
    public string VoterIp { get; set; }

    [StringLength(100)]
    public string VoterName { get; set; }  // For anonymous voting

    public bool RealTimeCount { get; set; } = true;

    [StringLength(255)]
    public string FilterOptions { get; set; }

    public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    
    [ForeignKey("QuestionId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual Question Question { get; set; }

    [ForeignKey("UserId")]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public virtual User User { get; set; }

    [ForeignKey("ChoiceId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual Choice Choice { get; set; }

    #region Constructor
    public VoteDetail()
    {
        Id = Guid.NewGuid();
        VotedAt = DateTime.UtcNow;
        RealTimeCount = true;
    }

    public VoteDetail(Guid questionId, Guid userId, Guid choiceId, string textAnswer, string voterIp, string voterName, bool realTimeCount, string filterOptions, string createdBy)
    {
        Id = Guid.NewGuid();
        QuestionId = questionId;
        UserId = userId;
        ChoiceId = choiceId;
        TextAnswer = textAnswer;
        VoterIp = voterIp;
        VoterName = voterName;
        RealTimeCount = realTimeCount;
        FilterOptions = filterOptions;
        VotedAt = DateTime.UtcNow;
        UserNameCreated = createdBy;
    }
    #endregion

    #region Business Logic
    public void UpdateVoteDetail(Guid choiceId, string textAnswer, string voterIp, string voterName, bool realTimeCount, string filterOptions, string updatedBy)
    {
        ChoiceId = choiceId;
        TextAnswer = textAnswer;
        VoterIp = voterIp;
        VoterName = voterName;
        RealTimeCount = realTimeCount;
        FilterOptions = filterOptions;
        UpdatedDateTime = DateTime.UtcNow;
        UserNameUpdated = updatedBy;
    }

    public bool ValidateVoteDetail()
    {
        if (string.IsNullOrWhiteSpace(QuestionId.ToString()))
        {
            throw new ArgumentException("Question ID cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(VoterIp))
        {
            throw new ArgumentException("Voter IP cannot be empty.");
        }

        return true;
    }
    #endregion
}
