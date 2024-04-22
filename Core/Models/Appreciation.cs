using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Models
{
    public class Appreciation
    {
        [Key]
        public Guid Id { get; set; }
        [Display(Name = "Date Requested")]
        public DateTime DateAppreciated { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal Amount { get; set; }
        public string UserId { get; set; }
        [Display(Name = "User")]
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        [Display(Name = " Appreciated By ")]
        public string AppreciatedBy { get; set; }
        [Display(Name = "Description ")]
        public string AppreciationDetails { get; set; }
        [NotMapped]
        [Display(Name = "Member Username ")]
        public string MemberUsername { get; set; }
    }
}
