namespace FDS.CRM.Domain.Entities;

public class PaymentMethod : Entity<Guid>
{
    [StringLength(100)]
    public string PaymentMethodName { get; private set; }
    [StringLength(500)]
    public string Description { get; private set; }

    #region Constructor
    private PaymentMethod(string paymentMethodName, string description)
    {
        Id = Guid.NewGuid();
        PaymentMethodName = paymentMethodName;
        Description = description;
    }
    #endregion

    #region Business Logic
    public static PaymentMethod Create(string paymentMethodName, string description)
    {
        return new PaymentMethod(paymentMethodName, description);
    }
    #endregion
}
