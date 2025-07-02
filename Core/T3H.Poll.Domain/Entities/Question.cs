namespace T3H.Poll.Domain.Entities;

public class Question : Entity<Guid>, IAggregateRoot
{
    [Required]
    public Guid PollId { get; set; }
    public Poll Poll { get; set; }

    [Required]
    public string QuestionText { get; set; }

    [Required]
    public QuestionType QuestionType { get; set; }

    public bool IsRequired { get; set; }

    public int QuestionOrder { get; set; }

    [StringLength(255)]
    public string? MediaUrl { get; set; }
    
    public string? Settings { get; set; }

    public List<Choice> Choices { get; set; }
    
    public bool IsActive { get; set; }

    #region Constructor
    public Question()
    {
        Id = Guid.NewGuid();
        CreatedDateTime = DateTimeOffset.UtcNow;
        IsRequired = true;
        QuestionOrder = 0;
        Choices = new List<Choice>();
    }

    public Question(Guid pollId, string questionText, QuestionType questionType, bool isRequired, int questionOrder, string mediaUrl, string settings)
    {
        Id = Guid.NewGuid();
        PollId = pollId;
        QuestionText = questionText;
        QuestionType = questionType;
        IsRequired = isRequired;
        QuestionOrder = questionOrder;
        MediaUrl = mediaUrl;
        Settings = settings;
        CreatedDateTime = DateTimeOffset.UtcNow;
        Choices = new List<Choice>();
        IsActive = true;
    }
    #endregion

    #region Business Logic
    public void UpdateQuestion(string questionText, QuestionType questionType, bool isRequired, int questionOrder, string mediaUrl, string settings)
    {
        QuestionText = questionText;
        QuestionType = questionType;
        IsRequired = isRequired;
        QuestionOrder = questionOrder;
        MediaUrl = mediaUrl;
        Settings = settings;
        UpdatedDateTime = DateTimeOffset.UtcNow;
    }

    public bool ValidateQuestion()
    {
        if (string.IsNullOrWhiteSpace(QuestionText))
        {
            throw new ArgumentException("Question text cannot be empty.");
        }

        if (!Enum.IsDefined(typeof(QuestionType), QuestionType))
        {
            throw new ArgumentException("Invalid question type.");
        }

        if (PollId == Guid.Empty)
        {
            throw new ArgumentException("Poll ID cannot be empty.");
        }

        return true;
    }

    public List<Choice> GetActiveChoices()
    {
        return Choices.Where(c => c.IsActive == true).ToList();
    }
    #endregion

    public void DeactivateQuestion()
    {
        IsActive = false;
        UpdatedDateTime = DateTimeOffset.UtcNow;
    }
}