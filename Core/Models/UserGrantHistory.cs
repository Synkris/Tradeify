using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Models
{
    public class UserGrantHistory
    {
        public Guid Id { get; set; }  

        [Display(Name = "Date Grant Earned")]
        public DateTime DateEarned { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal AmountEarned { get; set; }

        public string UserId { get; set; }
        [Display(Name = "User")]
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public Guid? RegFeeGrantId { get; set; }
        [Display(Name = "RegFeeGrant")]
        [ForeignKey("RegFeeGrantId")]
        public virtual RegFeeGrants RegFeeGrants { get; set; }
    }
}
