namespace T3H.Poll.Application.Common.DTOs;

public class SearchRequestModel
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortField { get; set; }
    public bool IsDescending { get; set; }
    public List<SearchCondition> Conditions { get; set; } = new List<SearchCondition>();
}
