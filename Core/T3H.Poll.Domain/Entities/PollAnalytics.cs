namespace T3H.Poll.Domain.Entities;

using System;
using System.ComponentModel.DataAnnotations.Schema;

public class PollAnalytics : Entity<Guid>, IAggregateRoot
{
    [Required]
    public Guid PollId { get; set; }
    public int? TotalViews { get; set; }
    public int? TotalVotes { get; set; }
    public decimal? AverageCompletionTime { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? CompletionRate { get; set; }  // Nullable to allow for no data

    public string DemographicsData { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    [ForeignKey("PollId")]
    public virtual Poll Poll { get; set; }

    #region Constructor
    public PollAnalytics()
    {
        Id = Guid.NewGuid();
        LastUpdated = DateTime.UtcNow;
    }

    public PollAnalytics(Guid pollId, int totalViews, int totalVotes, int? averageCompletionTime, decimal? completionRate, string demographicsData, string createdBy)
    {
        Id = Guid.NewGuid();
        PollId = pollId;
        TotalViews = totalViews;
        TotalVotes = totalVotes;
        AverageCompletionTime = averageCompletionTime;
        CompletionRate = completionRate;
        DemographicsData = demographicsData;
        LastUpdated = DateTime.UtcNow;
        UserNameCreated = createdBy;
    }
    #endregion

    #region Business Logic
    public void UpdateAnalytics(int? totalViews, int? totalVotes, decimal? averageCompletionTime, decimal? completionRate, string demographicsData, string updatedBy)
    {
        TotalViews = totalViews;
        TotalVotes = totalVotes;
        AverageCompletionTime = averageCompletionTime;
        CompletionRate = completionRate;
        DemographicsData = demographicsData;
        LastUpdated = DateTime.UtcNow;
        UserNameUpdated = updatedBy;
    }

    public bool ValidateAnalytics()
    {
        if (TotalViews.HasValue && TotalViews < 0)
        {
            throw new ArgumentException("Total views cannot be negative.");
        }

        if (TotalVotes.HasValue && TotalVotes < 0)
        {
            throw new ArgumentException("Total votes cannot be negative.");
        }

        if (AverageCompletionTime.HasValue && AverageCompletionTime < 0)
        {
            throw new ArgumentException("Average completion time cannot be negative.");
        }

        if (CompletionRate.HasValue && (CompletionRate < 0 || CompletionRate > 100))
        {
            throw new ArgumentException("Completion rate must be between 0 and 100.");
        }
        if (string.IsNullOrWhiteSpace(PollId.ToString()))
        {
            throw new ArgumentException("Poll ID cannot be empty.");
        }

        return true;
    }
    #endregion
}
