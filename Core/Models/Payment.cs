using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Payment
    {
        public Guid Id { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal Amount { get; set; }
        public int PaymentTypeId { get; set; }
        [Display(Name = "Payment")]
        [ForeignKey("PaymentOptionId")]
        public virtual CommonDropdowns PaymentOption { get; set; }
        [Display(Name = "Date Of Payment")]
        public DateTime DateGenerated { get; set; }
        public int PackagesId { get; set; }
        [Display(Name = "Packages")]
        [ForeignKey("PackagesId")]
        public virtual Packages Packages { get; set; }
        public string UserId { get; set; }
        [Display(Name = "User")]
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        public string InvoiceNumber { get; set; }
        public PaymentStatus PaymentStatus { get; set; }

        [Display(Name = "Bank Account Paid To")]
        public int? BankAccountId { get; set; }
        [Display(Name = "Bank Account")]
        [ForeignKey("BankAccountId")]
        public virtual CommonDropdowns BankAccount { get; set; }

        [Display(Name = "Account Name Paid From")]
        public string PaidFrom { get; set; }

        [Display(Name = "Upload")]
        public string FileUpload { get; set; }
    }

    public enum PaymentStatus
    {
        [Description("For Unpaid")]
        Unpaid = 1,
        [Description("For Paid")]
        Paid = 2,

    }
}
