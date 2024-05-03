using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels
{
    public class PasswordResetViewmodel
    {
        [Key]
        public Guid Token { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }


        [DataType(DataType.Password)]
        [Required]
        [Compare("Password", ErrorMessage = "Password and Confirm Password must be the same. ")]
        public string ConfirmPassword { get; set; }

    }
}
