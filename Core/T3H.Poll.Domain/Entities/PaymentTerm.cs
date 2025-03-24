namespace FDS.CRM.Domain.Entities;

// điều khoản thanh toán
public class PaymentTerm : Entity<Guid>
{
    [StringLength(200)]
    [Required]
    public string Name { get; private set; }
    public int Sortby { get; private set; }

    #region Constructor
    private PaymentTerm(string name, int sortby)
    {
        Id = Guid.NewGuid();
        Name = name;
        Sortby = sortby;
    }
    #endregion

    #region Business Logic
    public static PaymentTerm Create(string name, int sortby)
    {
        return new PaymentTerm(name, sortby);
    }
    #endregion
}
