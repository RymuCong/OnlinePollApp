namespace T3H.Poll.Application.Question.Queries;

public class SearchQuestionQueryParams
{
    /// <summary>
    /// Tìm kiếm theo nội dung câu hỏi
    /// </summary>
    public string? QuestionText { get; set; }

    /// <summary>
    /// Lọc theo loại câu hỏi (MultipleChoice, SingleChoice, Text, etc.)
    /// </summary>
    public string? QuestionType { get; set; }

    /// <summary>
    /// Lọc theo Poll ID cụ thể
    /// </summary>
    public Guid? PollId { get; set; }

    /// <summary>
    /// Lọc câu hỏi bắt buộc hoặc không bắt buộc
    /// </summary>
    public bool? IsRequired { get; set; }

    /// <summary>
    /// Lọc câu hỏi đang hoạt động hoặc không hoạt động
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Lọc theo người tạo (thông qua Poll CreatorId)
    /// </summary>
    public Guid? CreatorId { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortField { get; set; } = "QuestionOrder";
    public bool IsDescending { get; set; } = false;

    public SearchRequestModel ToSearchRequest()
    {
        var conditions = new List<SearchCondition>();

        // Handle question text search
        if (!string.IsNullOrEmpty(QuestionText))
        {
            conditions.Add(new SearchCondition { Field = "QuestionText", Operator = "contains", Value = QuestionText });
        }

        // Question type filtering
        if (!string.IsNullOrEmpty(QuestionType))
        {
            conditions.Add(new SearchCondition { Field = "QuestionType", Operator = "eq", Value = QuestionType });
        }

        // Poll ID filtering
        if (PollId.HasValue)
        {
            conditions.Add(new SearchCondition { Field = "PollId", Operator = "eq", Value = PollId.Value.ToString() });
        }

        // IsRequired filtering
        if (IsRequired.HasValue)
        {
            conditions.Add(new SearchCondition { Field = "IsRequired", Operator = "eq", Value = IsRequired.Value.ToString() });
        }

        // IsActive filtering
        if (IsActive.HasValue)
        {
            conditions.Add(new SearchCondition { Field = "IsActive", Operator = "eq", Value = IsActive.Value.ToString() });
        }

        // Creator filtering (through Poll relationship)
        if (CreatorId.HasValue)
        {
            conditions.Add(new SearchCondition { Field = "CreatorId", Operator = "eq", Value = CreatorId.Value.ToString() });
        }

        return new SearchRequestModel
        {
            PageNumber = PageNumber > 0 ? PageNumber : 1,
            PageSize = PageSize > 0 ? PageSize : 10,
            SortField = !string.IsNullOrEmpty(SortField) ? SortField : "QuestionOrder",
            IsDescending = IsDescending,
            Conditions = conditions
        };
    }
}