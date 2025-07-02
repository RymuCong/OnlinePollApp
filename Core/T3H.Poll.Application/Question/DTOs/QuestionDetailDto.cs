namespace T3H.Poll.Application.Question.DTOs;

public class QuestionDetailDto
{
    public Guid Id { get; set; }
    public Guid PollId { get; set; }
    public string QuestionText { get; set; }
    public string QuestionType { get; set; }
    public bool IsRequired { get; set; }
    public int QuestionOrder { get; set; }
    public string? MediaUrl { get; set; }
    public string? Settings { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public DateTime? UpdatedDateTime { get; set; }
    
    // Poll information
    public string? PollTitle { get; set; }
    public string? PollDescription { get; set; }
    public Guid? PollCreatorId { get; set; }
    public bool PollIsActive { get; set; }
    
    // Related data
    public List<ChoiceDetailDto> Choices { get; set; } = new();
    public List<RelatedQuestionDto> RelatedQuestions { get; set; } = new();
}

public class ChoiceDetailDto
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public string ChoiceText { get; set; }
    public int? ChoiceOrder { get; set; }
    public bool? IsCorrect { get; set; }
    public string? MediaUrl { get; set; }
    public bool? IsActive { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public DateTime? UpdatedDateTime { get; set; }
}

public class RelatedQuestionDto
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; }
    public string QuestionType { get; set; }
    public int QuestionOrder { get; set; }
    public bool IsRequired { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDateTime { get; set; }
}