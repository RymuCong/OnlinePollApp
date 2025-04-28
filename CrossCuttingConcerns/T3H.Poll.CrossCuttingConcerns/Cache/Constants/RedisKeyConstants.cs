namespace T3H.Poll.CrossCuttingConcerns.Cache.Constants
{

    // Nơi lưu trữ key tập trung
    public static class RedisKeyConstants
    {
        public const string GetPollsByUserId = "PollsByUserId";
        public const string GetCustomerByCodeAndVersion = "GetCustomerByCodeAndVersion";
    }
}
