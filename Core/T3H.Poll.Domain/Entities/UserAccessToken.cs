namespace T3H.Poll.Domain.Entities
{
    // Lưu trữ các thông tin của AccessToken và RefreshToken của cái user đăng nhập
    public class UserAccessToken : Entity<Guid>
    {
        [StringLength(200)]
        public string? RefreshToken { get; set; }
        [StringLength(100)]
        public string? ClientIp { get; set; }
        [StringLength(500)]
        public string? UserAgent { get; set; }
        [StringLength(200)]
        public string? FamilyId { get; set; }

        public bool IsBlocked { get; set; }

        public Guid UserId { get; set; }

        public User? User { get; set; }

        public DateTimeOffset ExpiredTime { get; set; }
    }
}
