using System;

namespace T3H.Poll.Domain.Entities;

public class RoleClaim : Entity<Guid>
{
    public string Type { get; set; }
    public string Value { get; set; }

    public Role Role { get; set; }
}
