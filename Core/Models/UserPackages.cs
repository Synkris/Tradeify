using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class UserPackages : BaseModel
    {
        [Key]
        public new Guid Id { get; set; }

        [Display(Name = "Package Name")]
        public new string Name { get; set; }

        public string UserId { get; set; }
        [Display(Name = "User")]
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Display(Name = "Package Amount")]
        public decimal Amount { get; set; }

        [Display(Name = "Crypto Coin Paid ")]
        public string? CryptoAmount { get; set; }

        [Display(Name = "Date Of Payment")]
        public DateTime? DateOfPayment { get; set; }

        [Display(Name = "Date Of Admin-Approval")]
        public DateTime? DateOfApproval { get; set; }

        [Display(Name = "Date Bonus Paid & Sent")]
        public DateTime? DateBonusPaid { get; set; }

        [Display(Name = "Max Generation")]
        public int MaxGeneration { get; set; }
        public Guid? PaymentId { get; set; }
        [Display(Name = "PaymentId")]
        [ForeignKey("PaymentId")]
        public virtual PaymentForm Payment { get; set; }
        public int PackageId { get; set; }
        [Display(Name = "Package")]
        [ForeignKey("PackageId")]
        public virtual Packages Package { get; set; }
        [Display(Name = "Withdrawal Status")]
        public bool IsMatured { get; set; }
    }
}
