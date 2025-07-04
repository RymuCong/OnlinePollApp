﻿namespace T3H.Poll.Application.Question.DTOs;

using System;
using System.Collections.Generic;

public class QuestionDto
{
    public Guid Id { get; set; }
    public Guid PollId { get; set; }
    public string QuestionText { get; set; }
    public string QuestionType { get; set; }
    public bool IsRequired { get; set; }
    public int QuestionOrder { get; set; }
    public string MediaUrl { get; set; }
    public string Settings { get; set; }
    public List<ChoiceDto> Choices { get; set; } = new List<ChoiceDto>();
    public DateTimeOffset CreatedDateTime { get; set; }
    public string UserNameCreated { get; set; }
    public DateTimeOffset? UpdatedDateTime { get; set; }
    public string UserNameUpdated { get; set; }
}

public class UpdateQuestionDto
{
    public Guid Id { get; set; }
    public string? QuestionText { get; set; }
    public string? QuestionType { get; set; }
    public bool? IsRequired { get; set; }
    public int? QuestionOrder { get; set; }
    public string? MediaUrl { get; set; }
    public string? Settings { get; set; }
    public bool? IsActive { get; set; }
    public ICollection<UpdateChoiceDto>? Choices { get; set; }
}

public class UpdateChoiceDto
{
    public string? Id { get; set; } 
    public string? ChoiceText { get; set; }
    public int? ChoiceOrder { get; set; }
    public bool? IsCorrect { get; set; }
    public string? MediaUrl { get; set; }
    public bool? IsActive { get; set; }
}
