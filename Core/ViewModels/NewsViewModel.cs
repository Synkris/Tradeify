using Core.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Core.ViewModels
{
    public class NewsViewModel
    {
        public string Name { get; set; }

        [Required(ErrorMessage = " Please enter more details")]
        [Display(Name = "Details")]
        public string Details { get; set; }

        [Required(ErrorMessage = " Please select date")]
        [Display(Name = "Date Created")]
        public DateTime DateCreated { get; set; }

        [Required(ErrorMessage = " Please select user")]
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Required(ErrorMessage = "Please choose a news image")]
        [Display(Name = "Master Image Url")]
        public IFormFile MasterImage { get; set; }

        public string ShortDetails { get; set; }
        public string RedBy { get; set; }

    }
}
