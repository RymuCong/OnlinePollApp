namespace FDS.CRM.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

public class Poll : Entity<Guid>, IAggregateRoot
{
    [Required]
    [StringLength(255)]
    public string Title { get; set; }

    public string Description { get; set; }

    [Required]
    public string CreatorId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public bool IsActive { get; set; }

    public bool IsAnonymous { get; set; }

    public bool IsMultipleVotesAllowed { get; set; }

    public bool IsViewableByModerator { get; set; }

    public bool IsPublic { get; set; }

    public string AccessCode { get; set; }

    public string ThemeSettings { get; set; }

    [StringLength(20)]
    public string VotingFrequencyControl { get; set; }

    public int VotingCooldownMinutes { get; set; }

    public List<Question> Questions { get; set; }

    #region Constructor
    public Poll()
    {
        Id = Guid.NewGuid();
        CreatedDateTime = DateTime.UtcNow;
        StartTime = DateTime.UtcNow;
        IsActive = true;
        IsAnonymous = false;
        IsMultipleVotesAllowed = false;
        IsViewableByModerator = true;
        IsPublic = true;
        VotingFrequencyControl = "once";
        VotingCooldownMinutes = 0;
        Questions = new List<Question>();
    }

    public Poll(string title, string description, string creatorId, DateTime startTime, DateTime? endTime, bool isActive, bool isAnonymous, bool isMultipleVotesAllowed, bool isViewableByModerator, bool isPublic, string accessCode, string themeSettings, string votingFrequencyControl, int votingCooldownMinutes, string updatedBy)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        CreatedDateTime = DateTime.UtcNow;
        CreatorId = creatorId;
        StartTime = startTime;
        EndTime = endTime;
        IsActive = isActive;
        IsAnonymous = isAnonymous;
        IsMultipleVotesAllowed = isMultipleVotesAllowed;
        IsViewableByModerator = isViewableByModerator;
        IsPublic = isPublic;
        AccessCode = accessCode;
        ThemeSettings = themeSettings;
        VotingFrequencyControl = votingFrequencyControl;
        VotingCooldownMinutes = votingCooldownMinutes;
        UpdatedDateTime = DateTime.UtcNow;
        UserNameUpdated = updatedBy;
        Questions = new List<Question>();
    }
    #endregion

    #region Business Logic
    public void UpdatePoll(string title, string description, DateTime? endTime, bool isActive, bool isAnonymous, bool isMultipleVotesAllowed, bool isViewableByModerator, bool isPublic, string accessCode, string themeSettings, string votingFrequencyControl, int votingCooldownMinutes, string updatedBy)
    {
        Title = title;
        Description = description;
        EndTime = endTime;
        IsActive = isActive;
        IsAnonymous = isAnonymous;
        IsMultipleVotesAllowed = isMultipleVotesAllowed;
        IsViewableByModerator = isViewableByModerator;
        IsPublic = isPublic;
        AccessCode = accessCode;
        ThemeSettings = themeSettings;
        VotingFrequencyControl = votingFrequencyControl;
        VotingCooldownMinutes = votingCooldownMinutes;
        UpdatedDateTime = DateTime.UtcNow;
        UserNameUpdated = updatedBy;
    }

    public bool ValidatePoll()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            throw new ArgumentException("Title cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(CreatorId))
        {
            throw new ArgumentException("Creator ID cannot be empty.");
        }

        if (EndTime.HasValue && EndTime < StartTime)
        {
            throw new ArgumentException("End time cannot be earlier than start time.");
        }

        return true;
    }

    public List<Question> GetActiveQuestions()
    {
        return Questions.Where(q => q.IsRequired).ToList();
    }
    #endregion
}