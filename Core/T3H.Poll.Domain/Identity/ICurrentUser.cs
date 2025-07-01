using System;

namespace T3H.Poll.Domain.Identity;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    Guid UserId { get; }
    
    string Email { get; }
}
