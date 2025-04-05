using System.ComponentModel.DataAnnotations.Schema;

namespace T3H.Poll.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

public class Poll : Entity<Guid>, IAggregateRoot
{
    [Required]
    [StringLength(255)]
    public string Title { get; set; }

    public string? Description { get; set; }
    
    [ForeignKey("User")]
    public Guid CreatorId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public bool IsActive { get; set; }

    public bool IsAnonymous { get; set; }

    public bool IsMultipleVotesAllowed { get; set; }

    public bool IsViewableByModerator { get; set; }

    public bool IsPublic { get; set; }

    public string? AccessCode { get; set; }

    public string? ThemeSettings { get; set; }

    [StringLength(20)]
    public string? VotingFrequencyControl { get; set; }

    public int VotingCooldownMinutes { get; set; }

    public List<Question> Questions { get; set; } = new List<Question>();
    
    #region Business Logic
    public static Poll Create(string title, string description, Guid creatorId, DateTime startTime, DateTime? endTime, bool isActive, bool isAnonymous, bool isMultipleVotesAllowed, bool isViewableByModerator, bool isPublic, string accessCode, string votingFrequencyControl, int votingCooldownMinutes)
    {
        return new Poll
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            CreatorId = creatorId,
            StartTime = startTime,
            EndTime = endTime,
            IsActive = isActive,
            IsAnonymous = isAnonymous,
            IsMultipleVotesAllowed = isMultipleVotesAllowed,
            IsViewableByModerator = isViewableByModerator,
            IsPublic = isPublic,
            AccessCode = accessCode,
            VotingFrequencyControl = votingFrequencyControl,
            VotingCooldownMinutes = votingCooldownMinutes,
            CreatedDateTime = DateTimeOffset.Now,
            UpdatedDateTime = DateTimeOffset.Now,
            UserNameCreated = "System"
        };
    }
    
    #endregion
}