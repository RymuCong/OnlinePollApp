namespace T3H.Poll.Application.Polls.Queries;

public class SearchPublicPollsQueryParams 
{
    public string? SearchField { get; set; } = string.Empty; // Search theo tên hoặc mô tả của Poll
    public Guid? CreatorId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortField { get; set; } = "UpdatedDateTime";
    public bool IsDescending { get; set; } = true;

    public SearchRequestModel ToSearchRequestModel()
    {
        var conditions = new List<SearchCondition>();
        
        if (!string.IsNullOrEmpty(SearchField))
        {
            conditions.Add(new SearchCondition
            {
                Field = "FieldSearch",
                Operator = "contains",
                Value = SearchField
            });
        }
        
        if (CreatorId.HasValue)
        {
            conditions.Add(new SearchCondition
            {
                Field = "CreatorId",
                Operator = "eq",
                Value = CreatorId.Value.ToString()
            });
        }
        
        return new SearchRequestModel
        {
            PageNumber = Page > 0 ? Page : 1,
            PageSize = PageSize > 0 ? PageSize : 10,
            SortField = !string.IsNullOrEmpty(SortField) ? SortField : "UpdatedDateTime",
            IsDescending = IsDescending,
            Conditions = conditions
        };
    }
}