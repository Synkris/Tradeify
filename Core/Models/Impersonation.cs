using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Impersonation
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Impersonator Id")]
        public string AdminUserId { get; set; }


        [Display(Name = "Impersonatee Id")]
        public string AdifMemberId { get; set; }


        [Display(Name = "Date Impersonated")]
        public DateTime DateImpersonated { get; set; }


        [Display(Name = "Is Impersonation session Ended")]
        public bool EndSession { get; set; }

        [Display(Name = "Date Impersonation Session Ended")]
        public DateTime DateSessionEnded { get; set; }


        [Display(Name = "Is checking if session is on and logs in the real user")]
        public bool AmTheRealUser { get; set; }
    }
}
