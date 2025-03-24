namespace FDS.CRM.Application.Common.DTOs;

public class SearchCondition
{
    public string Field { get; set; }
    public string Operator { get; set; }
    public object Value { get; set; }
}

public class SearchResponseModel<T>
{
    public List<T> Items { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
}
