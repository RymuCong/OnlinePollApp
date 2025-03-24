using System.ComponentModel.DataAnnotations.Schema;

namespace FDS.CRM.Domain.Entities;

public class PollInvitation : Entity<Guid>
{
    [Required]
    public Guid PollId { get; set; }

    [Required]
    [StringLength(100)]
    public string Email { get; set; }

    [Required]
    [StringLength(100)]
    public string InvitationToken { get; set; }

    public bool IsAccepted { get; set; } = false;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public DateTime? AcceptedAt { get; set; }

    [ForeignKey("PollId")]
    public virtual Poll Poll { get; set; }

    [ForeignKey("CreatedBy")]
    public virtual User User { get; set; }

    #region Constructor
    public PollInvitation()
    {
        Id = Guid.NewGuid();
        SentAt = DateTime.UtcNow;
        IsAccepted = false;
    }

    public PollInvitation(Guid pollId, string email, string invitationToken)
    {
        Id = Guid.NewGuid();
        PollId = pollId;
        Email = email;
        InvitationToken = invitationToken;
        SentAt = DateTime.UtcNow;
        IsAccepted = false;
    }
    #endregion

    #region Business Logic
    public void AcceptInvitation()
    {
        IsAccepted = true;
        AcceptedAt = DateTime.UtcNow;
    }

    public bool ValidateInvitation()
    {
        if (string.IsNullOrWhiteSpace(PollId.ToString()))
        {
            throw new ArgumentException("Poll ID cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            throw new ArgumentException("Email cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(InvitationToken))
        {
            throw new ArgumentException("Invitation token cannot be empty.");
        }

        return true;
    }
    #endregion
}