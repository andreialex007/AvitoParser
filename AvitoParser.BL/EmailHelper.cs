using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using NLog;

namespace AvitoParser.BL
{
    public class EmailHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static NameValueCollection AppSettings;

        private static string _smtpServer;
        private static int _smtpPort;
        private static string _adminEmail;
        private static string _loginEmail;
        private static string _loginPassword;
        private static bool _enableSsl;
        private static bool _htmlFormat;

        public static void SetConfig(NameValueCollection appSettings)
        {
            AppSettings = appSettings;
            Init();
        }

        private static void Init()
        {
            _smtpServer = AppSettings["SmtpServer"];
            _smtpPort = Convert.ToInt32(AppSettings["SmtpPort"]);
            _loginEmail = AppSettings["LoginEmail"];
            _adminEmail = AppSettings["AdminEmail"];
            _loginPassword = AppSettings["LoginPassword"];
            _enableSsl = Convert.ToBoolean(AppSettings["EnableSSL"]);
            _htmlFormat = Convert.ToBoolean(AppSettings["HtmlFormat"]);
        }

        public static void SendInfoEmail(string subject, string body)
        {
            Task.Run(() =>
            {
                SendEmail(subject, body, "AvitoParser@info.ru", "andreialexandrovich@gmail.com");
            });
        }

        public static void SendEmail(string subject, string body, string from, string to)
        {
            var mailMessage = new MailMessage(from, to, subject, body) { IsBodyHtml = true };
            SendEmail(mailMessage);
        }

        public static void SendEmail(MailMessage message, bool? isHtmlFormat = null)
        {
            isHtmlFormat = isHtmlFormat ?? _htmlFormat;
            message.IsBodyHtml = isHtmlFormat.Value;
            DoSendEmail(message);
        }

        private static void DoSendEmail(MailMessage message)
        {
            try
            {
                var client = new SmtpClient(_smtpServer, _smtpPort);
                client.EnableSsl = _enableSsl;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_loginEmail, _loginPassword);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Send(message);
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Error while sending email");
            }
        }
    }
}