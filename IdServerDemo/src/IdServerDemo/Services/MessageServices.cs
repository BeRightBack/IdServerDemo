using System;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using System.Threading.Tasks;
using System.Net; 

namespace IdServerDemo.Services
{
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link http://go.microsoft.com/fwlink/?LinkID=532713
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        public Task SendEmailAsync(string to, string from, string subject, string message)
        {
            // Credentials:
            var credentialUserName = Startup.Configuration["SmtpUser"];
            var pwd = Startup.Configuration["SmtpPwd"];
            var smtp = Startup.Configuration["SmtpSvr"];
            var email = Startup.Configuration["SiteEmailAddress"];

            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Administrator", credentialUserName));
            emailMessage.To.Add(new MailboxAddress("", to));
            emailMessage.Subject = subject;
            var builder = new BodyBuilder();
            builder.HtmlBody = string.Format(message);
            emailMessage.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {   // Creatte the credentials:
                NetworkCredential credentials =
                    new NetworkCredential(credentialUserName, pwd);
                client.LocalDomain = "sgpconcept.com";
                client.Connect(smtp, 25, SecureSocketOptions.Auto);
                client.Authenticate(credentials);
                client.Send(emailMessage);
                client.Disconnect(true);
            }

            return Task.FromResult(0); 
        }

        public Task SendSmsAsync(string number, string message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }
}
