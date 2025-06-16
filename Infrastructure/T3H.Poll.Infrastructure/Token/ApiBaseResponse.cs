namespace T3H.Poll.Infrastructure.Token;

[Serializable]
public class ApiBaseResponse
{
    public int StatusCode { get; set; }

    public string Message { get; set; }
}