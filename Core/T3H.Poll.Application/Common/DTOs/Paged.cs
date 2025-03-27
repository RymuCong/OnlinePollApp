namespace T3H.Poll.Application.Common.DTOs;

public class Paged<T>
{
    public long TotalItems { get; set; }

    public List<T> Items { get; set; }
}
