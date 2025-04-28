namespace T3H.Poll.Application.Common.DTOs;

public record ResultModel<T>(T Data, bool IsError = false, string ErrorMessage = default!, int Status = 200) where T : notnull
{
    public static ResultModel<T> Create(T? data, bool isError = false, string errorMessage = default!, int status = 200)
    {
        return new(data, isError, errorMessage, status);
    }
}

public record ListResultModel<T>(List<T> Items, long TotalItems, int CurrentPage, int PageSize, int TotalPages) where T : notnull
{
    public bool HasPrevious => CurrentPage > 1;
    
    public bool HasNext => CurrentPage < TotalPages;
    
    public static ListResultModel<T> Create(List<T> items, long totalItems = 0, int currentPage = 1, int pageSize = 20, int? totalPages = null)
    {
        // Validate inputs
        if (currentPage < 1)
            currentPage = 1;
            
        if (pageSize < 1)
            pageSize = 20;
            
        // Calculate total pages if not provided
        var calculatedTotalPages = totalPages ?? 
                                   (pageSize > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1);
            
        return new(
            items ?? new List<T>(), 
            totalItems, 
            currentPage, 
            pageSize, 
            calculatedTotalPages);
    }
}