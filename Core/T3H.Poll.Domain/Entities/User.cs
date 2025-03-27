namespace T3H.Poll.Domain.Entities;

public class User : Entity<Guid>, IAggregateRoot
{
    [StringLength(100)]
    public string UserName { get; set; }

    public string? NormalizedUserName { get; set; }
    [StringLength(100)]
    public string FullName { get; set; }

    [StringLength(100)]
    public string Email { get; set; }
    [StringLength(100)]
    public string? NormalizedEmail { get; set; }

    public bool EmailConfirmed { get; set; }
    [StringLength(500)]
    public string PasswordHash { get; set; }
    [StringLength(100)]
    public string PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }
    [StringLength(100)]
    public string? ConcurrencyStamp { get; set; }
    [StringLength(100)]
    public string? SecurityStamp { get; set; }

    public bool LockoutEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public int AccessFailedCount { get; set; }

    [StringLength(100)]
    public string? Auth0UserId { get; set; }
    [StringLength(100)]
    public string? AzureAdB2CUserId { get; set; }

    public IList<UserToken> Tokens { get; set; }

    public IList<UserClaim> Claims { get; set; }

    public IList<UserRole> UserRoles { get; set; }

    public IList<PasswordHistory> PasswordHistories { get; set; }
    [StringLength(100)]
    public string? ColorAvatar { get; set; }
    public Guid DepartmentId { get; set; }

}
