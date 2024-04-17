using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
   public class PvWalletHistory
   {
        public Guid Id { get; set; }
        public Guid WalletId { get; set; }
        [Display(Name = "Wallet")]
        [ForeignKey("WalletId")]
        public Wallet Wallet { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal Amount { get; set; }
        public DateTime DateOfTransaction { get; set; }
        public TransactionType TransactionType { get; set; }
        public Guid? PaymentId { get; set; }
        [Display(Name = "Payment")]
        [ForeignKey("PaymentId")]
        public PaymentForm Payment { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal NewBalance { get; set; }
        [NotMapped]
        public decimal PreviousBalance
        {
            get
            {
                if (TransactionType == TransactionType.Credit || TransactionType == TransactionType.Refund)
                {
                    return NewBalance - Amount;

                }
                else if (TransactionType == TransactionType.Debit)
                {
                    return NewBalance + Amount;
                }

                return decimal.Zero;
            }
        }
        public string Details { get; set; }
    }

}
