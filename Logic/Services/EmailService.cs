using Hangfire;
using Logic.Helpers;
using Logic.IHelpers;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Services
{


    public interface IEmailService
    {
        void CallHangfire(EmailMessage emailMessage);

        void SendEmail(string toEmail, string subject, string message);
        void SendEmailNonHangFire(string toEmail, string subject, string message);
        void Send(EmailMessage emailMessage);

     }

    public class EmailService : IEmailService
    {

        private readonly IEmailConfiguration _emailConfiguration;

        public EmailService(IEmailConfiguration emailConfiguration)
        {
            _emailConfiguration = emailConfiguration;
        }

        public void SendEmail(string toEmail, string subject, string message)
        {
            EmailAddress fromAddress = new EmailAddress()
            {
                Name = "Tradeify Support ",
                Address = _emailConfiguration.SmtpUsername,
            };

            List<EmailAddress> fromAddressList = new List<EmailAddress>
            {
                        fromAddress
            };
            EmailAddress toAddress = new EmailAddress()
            {
                Name = "Tradeify Support ",
                Address = toEmail
            };
            List<EmailAddress> toAddressList = new List<EmailAddress>
            {
                    toAddress
            };

            EmailMessage emailMessage = new EmailMessage()
            {
                FromAddresses = fromAddressList,
                ToAddresses = toAddressList,
                Subject = subject,
                Content = message


            };

            CallHangfire(emailMessage);
        }

        public void SendEmailNonHangFire(string toEmail, string subject, string message)
        {
            EmailAddress fromAddress = new EmailAddress()
            {
                Name = "Tradeify Support ",
                Address = _emailConfiguration.SmtpUsername,
            };

            List<EmailAddress> fromAddressList = new List<EmailAddress>
            {
                        fromAddress
            };
            EmailAddress toAddress = new EmailAddress()
            {
                Name = "Tradeify Support ",
                Address = toEmail
            };
            List<EmailAddress> toAddressList = new List<EmailAddress>
            {
                    toAddress
            };

            EmailMessage emailMessage = new EmailMessage()
            {
                FromAddresses = fromAddressList,
                ToAddresses = toAddressList,
                Subject = subject,
                Content = message

            };

            if (_emailConfiguration.SendEmail)
            {
                Send(emailMessage);
            }
        }
        public void CallHangfire(EmailMessage emailMessage)
		{
            if (_emailConfiguration.SendEmail)
            {
                BackgroundJob.Enqueue(() => Send(emailMessage));
            }
		}
		public void Send(EmailMessage emailMessage)
        {
            
            var message = new MimeMessage();
            message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));

            message.Subject = emailMessage.Subject;
            //We will say we are sending HTML. But there are options for plaintext etc. 
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = emailMessage.Content
            };

            //Be careful that the SmtpClient class is the one from Mailkit not the framework!
            using (var emailClient = new SmtpClient())
            {
                emailClient.ServerCertificateValidationCallback = (s, c, h, e) => true;

                // emailClient.Timeout = 30000;
                //emailClient.LocalDomain = "https://localhost:44300";
                //The last parameter here is to use SSL (Which you should!)

                emailClient.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, SecureSocketOptions.Auto);

                //Remove any OAuth functionality as we won't be using it. 
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");



                emailClient.Authenticate(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);

                emailClient.Send(message);

                emailClient.Disconnect(true);
            }
          
            
        }


    }

}
