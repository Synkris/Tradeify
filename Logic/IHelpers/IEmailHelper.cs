using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.IHelpers
{
    public interface IEmailHelper
    {
        void ForgotPasswordTemplateEmailer(ApplicationUser userEmail, string linkToClick);
        Task PasswordResetedTemplateEmailerAsync(ApplicationUser userEmail);
    }
}
