namespace T3H.Poll.Application.Polls.DTOs;

public class PollSubmissionRequest
{
    public Guid PollId { get; set; }
    public string? AccessCode { get; set; }
    public string? ParticipantEmail { get; set; }
    public string? ParticipantName { get; set; }
    public List<AnswerDto> Answers { get; set; } = new List<AnswerDto>();
}

public class AnswerDto
{
    public Guid QuestionId { get; set; }
    public string QuestionType { get; set; } = string.Empty;
    public object? Answer { get; set; } // Can be single value, array, or text
    public List<Guid>? SelectedChoiceIds { get; set; } // For multiple choice questions
    public string? TextAnswer { get; set; } // For text questions
    public Guid? SingleChoiceId { get; set; } // For single choice questions
}

public class PollSubmissionResponse
{
    public Guid SubmissionId { get; set; }
    public Guid PollId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset SubmittedAt { get; set; }
    public bool IsSuccessful { get; set; }
    public List<string> ValidationErrors { get; set; } = new List<string>();
}

public class PublicPollsRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
