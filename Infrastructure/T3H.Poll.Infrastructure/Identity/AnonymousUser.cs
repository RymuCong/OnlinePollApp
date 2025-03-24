using FDS.CRM.Domain.Identity;
using System;

namespace FDS.CRM.Infrastructure.Identity;

public class AnonymousUser : ICurrentUser
{
    public bool IsAuthenticated => false;

    public Guid UserId => Guid.Empty;
}
