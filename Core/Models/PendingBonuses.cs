using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Models
{
    public class PendingBonuses
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        [Display(Name = "User")]
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        public decimal Amount { get; set; }

    }
}
