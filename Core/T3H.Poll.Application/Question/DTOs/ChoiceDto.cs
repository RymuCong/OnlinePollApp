namespace T3H.Poll.Application.Question.DTOs;

using System;

public class ChoiceDto
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public string ChoiceText { get; set; }
    public int? ChoiceOrder { get; set; }
    public bool? IsCorrect { get; set; }
    public string? MediaUrl { get; set; }
    public bool? IsActive { get; set; }
    public DateTimeOffset CreatedDateTime { get; set; }
    public string UserNameCreated { get; set; }
    public DateTimeOffset? UpdatedDateTime { get; set; }
    public string UserNameUpdated { get; set; }
}
