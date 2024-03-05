using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Core.Models
{
    public class PaymentForm
    {
        [Key]
        public Guid Id { get; set; }

        [Display(Name = "Details")]
        public string Details { get; set; }

        [Display(Name = "Amount Paid ")]
        public decimal Amount { get; set; }

        [Display(Name = " Crypto Coin Paid ")]
        public string? CryptoAmount { get; set; }

        [Display(Name = "Date of Payment ")]
        public DateTime Date { get; set; }
        public string UserId { get; set; }
        [Display(Name = "User")]
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Display(Name = "Reg Fee Payment Status")]
        public Status Status { get; set; }

        [Display(Name = "Updated By")]
        public string? StatusBy { get; set; }

        [Display(Name = "Date of Update")]
        public DateTime? StatuseChangeDate { get; set; }

        [Display(Name = "Payment Type")]
        public PaymentType PaymentTypeId { get; set; }

        [Display(Name = "Bank Account Paid To")]
        public int? GCCAccountId { get; set; }
        [Display(Name = "GGC Account")]
        [ForeignKey("GGCAccountId")]
        public virtual CommonDropdowns GGCAccount { get; set; }

        [Display(Name = "Account Name  paid from")]
        public string PaidFrom { get; set; }

        [Display(Name = "Bank Name paid from")]
        public string BankNamePaidFrom { get; set; }
        [Display(Name = "Account Number  paid from")]
        public string AccountNumberPaidFrom { get; set; }
        public int PackageId { get; set; }
        [Display(Name = "Packages")]
        [ForeignKey("PackageId")]
        public virtual Packages Packages { get; set; }
        [Display(Name = "Mode Of Payment")]
        public string PaymentMethod { get; set; }
    }
    public enum Status
    {
        [Description("For Pending")]
        Pending = 1,
        [Description("For Approved")]
        Approved = 2,
        [Description("For Rejected")]
        Rejected = 3,
    }
    public enum PaymentType
    {
        [Description("Registration Fee")]
        RegistrationFee = 1,
        [Description("Package Fee")]
        PackageFee = 2,
        [Description("VTU Activation Fee")]
        VtuActivationFee = 3,
    }
}
