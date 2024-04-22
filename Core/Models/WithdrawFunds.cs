
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
	public class WithdrawFunds
   {
        public Guid Id { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Amount { get; set; }

        [Display(Name = "Date Of Request")]
        public DateTime DateRequested { get; set; }

        [Display(Name = "Date Of Approval")]
        public DateTime DateApprovedAndSent { get; set; }

        public string UserId { get; set; }
        [Display(Name = "User")]
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Display(Name = "Withdrawal Status")]
        public Status WithdrawStatus { get; set; }

        [Display(Name = "Withdrawal Type")]
        public string WithdrawalType { get; set; }

        [Display(Name = "Name of Creditor")]
        public string CreditedBy { get; set; }

        [Display(Name = "Bank Name")]
        public string BankAccountName { get; set; }

        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; }

        [Display(Name = "Account  Name")]
        public string AccountName { get; set; }

        [Display(Name = "Receiver's Full Name")]
        public string RequestedBy { get; set; }

	}
}
