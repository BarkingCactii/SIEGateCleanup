using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Configuration;

namespace SIEGateCleanup
{
    public class Email
    {
        private static readonly Logger.Logger _log = Logger.Log.GetInstance("log");

        public static void SendEmail(string message)
        {
            try
            {
                String mailServer = ConfigurationManager.AppSettings["MailServer"];
                String mailTo = ConfigurationManager.AppSettings["MailTo"];
                String mailFrom = ConfigurationManager.AppSettings["MailFrom"];
                String username = ConfigurationManager.AppSettings["Username"];
                String password = ConfigurationManager.AppSettings["Password"];
                int smtpPort = 587;
                if (!int.TryParse(ConfigurationManager.AppSettings["SMTPPort"], out smtpPort))
                    _log.Error(String.Format("SMTP Port not defined. Defaulting to {0}", smtpPort));

                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(mailServer);

                mail.IsBodyHtml = true;
                mail.From = new MailAddress(mailFrom);
                mail.To.Add(mailTo);
                String machine = System.Environment.MachineName;
                mail.Subject = machine + " Alert from " + System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                mail.Body = message;
                SmtpServer.Port = smtpPort;
                SmtpServer.Credentials = new System.Net.NetworkCredential(username, password);
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
        }
    }
}
