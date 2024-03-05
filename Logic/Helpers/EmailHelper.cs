using Core.DB;
using Core.Models;
using Logic.IHelpers;
using Logic.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Helpers
{
    public class EmailHelper : IEmailHelper
    {
        private readonly IEmailConfiguration _emailConfiguration;
        private readonly IEmailService _emailService;
        private readonly AppDbContext _context;

        public EmailHelper(IEmailConfiguration emailConfiguration, IEmailService emailService, AppDbContext context)
        {
            _emailConfiguration = emailConfiguration;
            _emailService = emailService;
            _context = context;
        }

        public async void ForgotPasswordTemplateEmailer(ApplicationUser userEmail, string linkToClick)
        {

            if (userEmail != null)
            {
                string toEmail = userEmail.Email;
                string subject = "Tradeify Password Reset";
                string message = "Hi " + userEmail.UserName + ", <br/> A password reset has been requested for your Tradeify Account, please click on the link below to create a new password <br>" +
                 "<br><a href=" + linkToClick + ">Change Password</a> <br/> or copy the link the link below to your browser </br>" + linkToClick +

                "<br/>" + "<br> If you have any trouble with your account, you can always email us at kristianmathiaz@gmail.com <br> Regards, <br> The GGC team <br> <br> If you didn't register for Tradeify, please ignore this message.";
                _emailService.SendEmail(toEmail, subject, message);

            }

        }

        public async Task PasswordResetedTemplateEmailerAsync(ApplicationUser userEmail)
        {

            if (userEmail != null)
            {
                string toEmail = userEmail.Email;

                string subject = "Tradeify Support";
                string message = "Hi " + userEmail.UserName + ", <br/> Your password has been changed." +

                "<br> If you did not make this change please email us at  " +
               "kristianmathiaz@gmail.com <br> Regards";

                _emailService.SendEmail(toEmail, subject, message);

            }
        }


    }
}
