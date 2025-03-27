using System.ComponentModel.DataAnnotations.Schema;

namespace T3H.Poll.Domain.Entities
{
    public class PurchaseTransaction : Entity<Guid>
    {
        public Guid ContactId { get; private set; }

        [ForeignKey("PaymentMethod")]
        public Guid BuyPaymentMethodId { get; private set; }
        public PaymentMethod PaymentMethod { get; private set; }

        [ForeignKey("PaymentTerm")]
        public Guid BuyPaymentTermId { get; private set; }
        public PaymentTerm PaymentTerm { get; private set; }

        [ForeignKey("User")]
        public Guid SaleId { get; private set; }
        public User User { get; private set; }

        #region Constructor
        private PurchaseTransaction(Guid contactId, Guid buyPaymentMethodId, Guid buyPaymentTermId
                                    /*Guid salePaymentMethodId, Guid salePaymentTermId*/, Guid saleId)
        {
            Id = Guid.NewGuid();
            ContactId = contactId;
            BuyPaymentMethodId = buyPaymentMethodId;
            BuyPaymentTermId = buyPaymentTermId;
            //SalePaymentMethodId = salePaymentMethodId;
            //SalePaymentTermId = salePaymentTermId;
            SaleId = saleId;
        }
        #endregion

        #region Business Logic
        public static PurchaseTransaction Create(Guid contactId, Guid buyPaymentMethodId, Guid buyPaymentTermId,
                                                 /*Guid salePaymentMethodId, Guid salePaymentTermId,*/ Guid saleId)
        {
            return new PurchaseTransaction(contactId, buyPaymentMethodId, buyPaymentTermId,/* salePaymentMethodId, salePaymentTermId,*/ saleId);
        }
        #endregion
    }
}
