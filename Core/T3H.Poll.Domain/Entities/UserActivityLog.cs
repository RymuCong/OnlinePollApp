using System.ComponentModel.DataAnnotations.Schema;

namespace FDS.CRM.Domain.Entities;

public class UserActivityLog : Entity<Guid>
{
    public Guid UserId { get; set; }  // Nullable for anonymous users

    [Required]
    [StringLength(50)]
    public string ActivityType { get; set; }  // 'login', 'create_poll', 'vote', 'view_results', etc.

    public string ActivityDetails { get; set; }  // JSON details of the activity

    [StringLength(50)]
    public string IpAddress { get; set; }

    [StringLength(255)]
    public string UserAgent { get; set; }  // Browser/device information

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [ForeignKey("UserId")]
    public virtual User User { get; set; }

    #region Constructor
    public UserActivityLog()
    {
        Id = Guid.NewGuid();
        Timestamp = DateTime.UtcNow;
    }

    public UserActivityLog(Guid userId, string activityType, string activityDetails, string ipAddress, string userAgent, string createdBy)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        ActivityType = activityType;
        ActivityDetails = activityDetails;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        Timestamp = DateTime.UtcNow;
        UserNameCreated = createdBy;
    }
    #endregion

    #region Business Logic
    public void UpdateActivityLog(string activityDetails, string ipAddress, string userAgent, string updatedBy)
    {
        ActivityDetails = activityDetails;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        UpdatedDateTime = DateTime.UtcNow;
        UserNameUpdated = updatedBy;
    }

    public bool ValidateActivityLog()
    {
        if (string.IsNullOrWhiteSpace(ActivityType))
        {
            throw new ArgumentException("Activity type cannot be empty.");
        }

        return true;
    }
    #endregion
}