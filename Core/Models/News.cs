using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Core.Models
{
    public class News : BaseModel
    {
        [Required(ErrorMessage = " Please enter more details")]
        [Display(Name = "Details")]
        public string Details { get; set; }


        [Required(ErrorMessage = " Please select date")]
        [Display(Name = "Date Created")]
        public DateTime DateCreated { get; set; }


        [Required(ErrorMessage = " Please select user")]
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Image")]
        public string?  MasterImageUrl { get; set; }
        [NotMapped]
        public string  Action { get; set; }
        public string? RedBy { get; set; }

        [NotMapped]
        public string ShortDetails
        {
            get
            {
                var details = Details;
                if (details != null && details.Length > 80)
                {
                   return Details.Substring(0, 80) + "....";
                }
                return details;
               
                
            }
            
        }
        
    }
}
