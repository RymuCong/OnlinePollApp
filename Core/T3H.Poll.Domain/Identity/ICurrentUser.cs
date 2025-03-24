using System;

namespace FDS.CRM.Domain.Identity;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    Guid UserId { get; }
}
