using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FDS.CRM.Domain.Entities;

public class PasswordHistory : Entity<Guid>
{
    [ForeignKey("User")]
    public Guid UserId { get; set; }

    public string PasswordHash { get; set; }

    public virtual User User { get; set; }
}