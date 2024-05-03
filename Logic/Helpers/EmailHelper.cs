using Core.DB;
using Core.Models;
using Logic.IHelpers;
using Logic.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Helpers
{
    public class EmailHelper : IEmailHelper
    {
        private readonly IEmailConfiguration _emailConfiguration;
        private readonly IEmailService _emailService;
        //public IUserHelper _userHelper;
        private readonly AppDbContext _context;

        public EmailHelper(IEmailConfiguration emailConfiguration, IEmailService emailService, /*IUserHelper userHelper,*/ AppDbContext context)
        {
            _emailConfiguration = emailConfiguration;
            _emailService = emailService;
           // _userHelper = userHelper;
            _context = context;
        }
        
        public async void ForgotPasswordTemplateEmailer(ApplicationUser userEmail, string linkToClick)
        {

            if (userEmail != null)
            {
                string toEmail = userEmail.Email;
                string subject = "GGC Password Reset";
                string message = "Hi " + userEmail.UserName + ", <br/> A password reset has been requested for your GGC Account, please click on the link below to create a new password <br>" +
                 "<br><a href=" + linkToClick + ">Change Password</a> <br/> or copy the link the link below to your browser </br>" + linkToClick +

                "<br/>" + "<br> If you have any trouble with your account, you can always email us at support@ggcprojects.com <br> Regards, <br> The GGC team <br> <br> If you didn't register for GGC, please ignore this message.";
                _emailService.SendEmail(toEmail, subject, message);

            }

        }

        public async Task PasswordResetedTemplateEmailerAsync(ApplicationUser userEmail)
        {

            if (userEmail != null)
            {
                string toEmail = userEmail.Email;

                string subject = "GGC Support";
                string message = "Hi " + userEmail.UserName + ", <br/> Your password has been changed." +

                "<br> If you did not make this change please email us at  " +
               "support@ggcprojects.com <br> Regards";

                _emailService.SendEmail(toEmail, subject, message);

            }
        }

    }
}
