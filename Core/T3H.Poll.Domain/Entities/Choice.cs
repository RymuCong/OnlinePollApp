namespace FDS.CRM.Domain.Entities;

using System;
using System.ComponentModel.DataAnnotations;

public class Choice : Entity<Guid>
{
    [Required]
    public Guid QuestionId { get; set; }

    [Required]
    public string ChoiceText { get; set; }

    public int? ChoiceOrder { get; set; }

    public bool? IsCorrect { get; set; }

    [StringLength(255)]
    public string MediaUrl { get; set; }

    public bool? IsActive { get; set; }

    #region Constructor
    public Choice()
    {
        Id = Guid.NewGuid();
        CreatedDateTime = DateTimeOffset.UtcNow;
        IsActive = true;
    }

    public Choice(Guid questionId, string choiceText, int? choiceOrder, bool? isCorrect, string mediaUrl, string createdBy)
    {
        Id = Guid.NewGuid();
        QuestionId = questionId;
        ChoiceText = choiceText;
        ChoiceOrder = choiceOrder;
        IsCorrect = isCorrect;
        MediaUrl = mediaUrl;
        CreatedDateTime = DateTimeOffset.UtcNow;
        UserNameCreated = createdBy;
        IsActive = true;
    }
    #endregion

    #region Business Logic
    public void UpdateChoice(string choiceText, int? choiceOrder, bool? isCorrect, string mediaUrl, string updatedBy)
    {
        ChoiceText = choiceText;
        ChoiceOrder = choiceOrder;
        IsCorrect = isCorrect;
        MediaUrl = mediaUrl;
        UpdatedDateTime = DateTimeOffset.UtcNow;
        UserNameUpdated = updatedBy;
    }

    public void DeactivateChoice()
    {
        IsActive = false;
        UpdatedDateTime = DateTimeOffset.UtcNow;
    }

    public bool ValidateChoice()
    {
        if (string.IsNullOrWhiteSpace(ChoiceText))
        {
            throw new ArgumentException("Choice text cannot be empty.");
        }

        if (ChoiceOrder.HasValue && ChoiceOrder < 0)
        {
            throw new ArgumentException("Choice order cannot be negative.");
        }

        return true;
    }
    #endregion
}