namespace T3H.Poll.Application.Common.DTOs;

public record ResultModel<T>(T Data, bool IsError = false, string ErrorMessage = default!, int Status = 200) where T : notnull
{
    public static ResultModel<T> Create(T? data, bool isError = false, string errorMessage = default!, int status = 200)
    {
        return new(data, isError, errorMessage, status);
    }
}

public record ListResultModel<T>(List<T> Items, long TotalItems, int Page, int PageSize) where T : notnull
{
    public static ListResultModel<T> Create(List<T> items, long totalItems = 0, int page = 1, int pageSize = 20)
    {
        return new(items, totalItems, page, pageSize);
    }
}
