using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdServerDemo.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string to, string from, string subject, string message);
    }
}
