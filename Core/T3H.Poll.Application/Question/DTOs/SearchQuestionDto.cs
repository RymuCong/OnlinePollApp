namespace T3H.Poll.Application.Question.DTOs;

public class QuestionSearchResponse
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
    public DateTimeOffset CreatedDateTime { get; set; }
    public DateTimeOffset? UpdatedDateTime { get; set; }
    public List<ChoiceSearchResponse> Choices { get; set; } = new List<ChoiceSearchResponse>();
}

public class ChoiceSearchResponse
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public string ChoiceText { get; set; }
    public int? ChoiceOrder { get; set; }
    public bool? IsCorrect { get; set; }
    public string? MediaUrl { get; set; }
    public bool? IsActive { get; set; }
}