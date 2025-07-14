namespace T3H.Poll.Infrastructure.Caching;

public class RedisKeyConstants
{
    public const string GetPollsByUserId = "polls:by-user-id";
    public const string GetPublicPolls = "polls:public";
}