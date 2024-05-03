using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class MatchingRequest
    {
        [Key]
        public Guid Id { get; set; }

        public string UserId { get; set; }
        [Display(Name = "User")]
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Display(Name = "Date Requested")]
        public DateTime DateRequested { get; set; }

        [Display(Name = "Matching Bonus Request Status")]
        public Status MatchingRequestStatus { get; set; }

        public string RightLegUsername { get; set; }

        public string LeftLegUsername { get; set; }

        [Display(Name = "Date Approved")]
        public DateTime DateApprovedByAdmin { get; set; }

        [Display(Name = " Approved By ")]
        public string ApprovedBy { get; set; }
    }
}
