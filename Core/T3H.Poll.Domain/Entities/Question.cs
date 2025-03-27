namespace T3H.Poll.Domain.Entities;

public class Question : Entity<Guid>
{
    [Required]
    public Guid VoteId { get; set; }

    [Required]
    public string QuestionText { get; set; }

    [Required]
    [StringLength(50)]
    public string QuestionType { get; set; }

    public bool IsRequired { get; set; }

    public int QuestionOrder { get; set; }

    [StringLength(255)]
    public string MediaUrl { get; set; }

    public string Settings { get; set; }

    public List<Choice> Choices { get; set; }

    #region Constructor
    public Question()
    {
        Id = Guid.NewGuid();
        CreatedDateTime = DateTimeOffset.UtcNow;
        IsRequired = true;
        QuestionOrder = 0;
        Choices = new List<Choice>();
    }

    public Question(Guid voteId, string questionText, string questionType, bool isRequired, int questionOrder, string mediaUrl, string settings, string createdBy)
    {
        Id = Guid.NewGuid();
        VoteId = voteId;
        QuestionText = questionText;
        QuestionType = questionType;
        IsRequired = isRequired;
        QuestionOrder = questionOrder;
        MediaUrl = mediaUrl;
        Settings = settings;
        CreatedDateTime = DateTimeOffset.UtcNow;
        UserNameCreated = createdBy;
        Choices = new List<Choice>();
    }
    #endregion

    #region Business Logic
    public void UpdateQuestion(string questionText, string questionType, bool isRequired, int questionOrder, string mediaUrl, string settings, string updatedBy)
    {
        QuestionText = questionText;
        QuestionType = questionType;
        IsRequired = isRequired;
        QuestionOrder = questionOrder;
        MediaUrl = mediaUrl;
        Settings = settings;
        UpdatedDateTime = DateTimeOffset.UtcNow;
        UserNameUpdated = updatedBy;
    }

    public bool ValidateQuestion()
    {
        if (string.IsNullOrWhiteSpace(QuestionText))
        {
            throw new ArgumentException("Question text cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(QuestionType))
        {
            throw new ArgumentException("Question type cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(VoteId.ToString()))
        {
            throw new ArgumentException("Vote ID cannot be empty.");
        }

        return true;
    }

    public List<Choice> GetActiveChoices()
    {
        return Choices.Where(c => c.IsActive == true).ToList();
    }
    #endregion
}