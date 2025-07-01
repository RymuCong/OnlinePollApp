using T3H.Poll.Domain.Identity;
using System;

namespace T3H.Poll.Infrastructure.Identity;

public class AnonymousUser : ICurrentUser
{
    public bool IsAuthenticated => false;

    public Guid UserId => Guid.Empty;
    
    public string Email => "Anonymous";
}
