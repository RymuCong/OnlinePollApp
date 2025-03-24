namespace FDS.CRM.Application.PurchaseTransaction;
public class PurchaseTransactionDto
{
    public Guid BuyPaymentMethodId { get; set; }
    public Guid BuyPaymentTermId { get; set; }
    //public Guid SalePaymentMethodId { get; set; }
    //public Guid SalePaymentTermId { get; set; }
    public Guid SaleId { get; set; }
}

public class PurchaseTransactionDetailDto
{
    public string BuyPaymentMethodName { get; set; }
    public string BuyPaymentTermName { get; set; }
    //public Guid SalePaymentMethodId { get; set; }
    //public Guid SalePaymentTermId { get; set; }
    public string SaleName { get; set; }
}
