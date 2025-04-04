﻿using System;

namespace T3H.Poll.Domain.Entities;

public class UserToken : Entity<Guid>
{
    public Guid UserId { get; set; }

    public string LoginProvider { get; set; }

    public string TokenName { get; set; }

    public string TokenValue { get; set; }
}
