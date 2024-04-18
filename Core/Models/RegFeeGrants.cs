using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Models
{
    public class RegFeeGrants
    {
        public Guid Id { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal GrantAmountPerUser { get; set; }

        [Display(Name = "Date Grant Made")]
        public DateTime GrantDate { get; set; }

        public Guid? PaymentId { get; set; }
        [Display(Name = "Payment")]
        [ForeignKey("PaymentId")]
        public virtual PaymentForm Payment { get; set; }
       

    }
}
