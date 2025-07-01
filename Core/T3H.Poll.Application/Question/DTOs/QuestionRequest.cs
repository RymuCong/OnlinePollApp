namespace T3H.Poll.Application.Question.DTOs;

using System;
using System.Collections.Generic;

public class QuestionRequest
{
    public string QuestionText { get; set; }
    public string QuestionType { get; set; } // Keep as string for API compatibility
    public bool IsRequired { get; set; }
    public int QuestionOrder { get; set; }
    public string? MediaUrl { get; set; }
    public string? Settings { get; set; }
    public List<ChoiceRequest> Choices { get; set; }
}

public class ChoiceRequest
{
    public Guid? Id { get; set; } // Optional for new choices or updates
    public string ChoiceText { get; set; }
    public int? ChoiceOrder { get; set; }
    public bool? IsCorrect { get; set; }
    public string? MediaUrl { get; set; }
    public bool? IsActive { get; set; } = true;
}
