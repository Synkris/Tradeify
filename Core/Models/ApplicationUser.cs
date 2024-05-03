using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Core.Models
{
    public class ApplicationUser : IdentityUser
   {
        [Display(Name = "First Name")]
        public virtual string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public virtual string LastName { get; set; }

        [Display(Name = "Name")]
        [NotMapped]
        public string Name => FirstName + " " + LastName;

        public int GenderId { get; set; }
        [Display(Name = "Gender")]
        [ForeignKey("GenderId")]
        public virtual CommonDropdowns Gender { get; set; }

        [Display(Name = "Registration Fee PaymentId")]
        public Guid? RegFeePaymentId { get; set; }
        [Display(Name = "RegFee Payment")]
        [ForeignKey("RegFeePaymentId")]
        public virtual PaymentForm RegFeePayment { get; set; }

        [Display(Name = "Registration Fee Paid")]
        public bool? RegFeePaid { get; set; }

        [NotMapped]
        [Display(Name = "Remember Me")]
        public bool RememberPassword { get; set; }
        public string RefferrerId { get; set; }
        [Display(Name = "Refferred By")]
        [ForeignKey("RefferrerId")]
        public virtual ApplicationUser Refferrer { get; set; }
        public string ParentId { get; set; }
        [Display(Name = "Parent ")]
        [ForeignKey("ParentId")]
        public virtual ApplicationUser Parent { get; set; }

        [NotMapped]
        [Display(Name = "Refferrer Username")]
        public string RefferrerUserName { get; set; }

        [NotMapped]
        [Display(Name = "Terms And Conditions")]
        [Required]
        public bool TermsAndConditions { get; set; }

        [Display(Name = "Date Registered")]
        public DateTime DateRegistered { get; set; }

        [Display(Name = "Last Login Time")]
        public DateTime CurrentLastLoginTime { get; set; }

        [Display(Name = "Last Logout")]
        public DateTime LastLogoutTime { get; set; }

        public bool Deactivated { get; set; }

		public int? LastGenPaid { get; set; }

		public int? LastPendingGen { get; set; }

        [Display(Name = "VTU Activation Fee Paid")]
        public bool VTUActivationFeePaid { get; set; }
        public string? CordinatorId { get; set; }
        [NotMapped]
        public string? DistributorId { get; set; }

        public virtual ICollection<UserPackages> UserPackages { get; set; }
        [NotMapped]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [NotMapped]
        [Display(Name = "ConfirmPassword")]
        public string ConfirmPassword { get; set; }
    }
}
