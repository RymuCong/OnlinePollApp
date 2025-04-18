﻿using System;

namespace T3H.Poll.Domain.Entities;

public class UserRole : Entity<Guid>
{
    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

    public User User { get; set; }

    public Role Role { get; set; }
}
