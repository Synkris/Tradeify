using System;
using System.Collections.Generic;
using System.Text;

namespace Logic.IHelpers
{
        public interface IEmailConfiguration
        {
            string SmtpServer { get; }
            int SmtpPort { get; }
            string SmtpUsername { get; set; }
            string SmtpPassword { get; set; }

            string PopServer { get; }
            int PopPort { get; }
            string PopUsername { get; }
            string PopPassword { get; }
            bool SendEmail { get; set; }
    }
}
