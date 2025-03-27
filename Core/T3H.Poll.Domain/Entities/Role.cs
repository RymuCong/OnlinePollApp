using System;
using System.Collections.Generic;

namespace T3H.Poll.Domain.Entities;

public class Role : Entity<Guid>, IAggregateRoot
{
    public virtual string Name { get; set; }

    public virtual string NormalizedName { get; set; }

    public virtual string ConcurrencyStamp { get; set; }

    public IList<RoleClaim> Claims { get; set; }

    public IList<UserRole> UserRoles { get; set; }
}
