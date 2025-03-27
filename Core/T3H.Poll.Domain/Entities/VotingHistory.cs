using System.ComponentModel.DataAnnotations.Schema;

namespace T3H.Poll.Domain.Entities
{
    public class VotingHistory : Entity<Guid>
    {
        [Required]
        public Guid PollId { get; set; }

        public Guid UserId { get; set; }  // Nullable for anonymous voting

        [StringLength(50)]
        public string VoterIp { get; set; }  // Used to track anonymous voters

        public DateTime VotedAt { get; set; } = DateTime.UtcNow;

        [StringLength(255)]
        public string UserAgent { get; set; }  // Browser/device information

        [ForeignKey("PollId")]
        public virtual Poll Poll { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        #region Constructor
        public VotingHistory()
        {
            Id = Guid.NewGuid();
            VotedAt = DateTime.UtcNow;
        }

        public VotingHistory(Guid pollId, Guid userId, string voterIp, string userAgent, string createdBy)
        {
            Id = Guid.NewGuid();
            PollId = pollId;
            UserId = userId;
            VoterIp = voterIp;
            UserAgent = userAgent;
            VotedAt = DateTime.UtcNow;
            UserNameCreated = createdBy;
        }
        #endregion

        #region Business Logic
        public void UpdateVotingHistory(string voterIp, string userAgent, string updatedBy)
        {
            VoterIp = voterIp;
            UserAgent = userAgent;
            UpdatedDateTime = DateTime.UtcNow;
            UserNameUpdated = updatedBy;
        }

        public bool ValidateVotingHistory()
        {
            if (string.IsNullOrWhiteSpace(PollId.ToString()))
            {
                throw new ArgumentException("Poll ID cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(VoterIp))
            {
                throw new ArgumentException("Voter IP cannot be empty.");
            }

            return true;
        }
        #endregion
    }
}