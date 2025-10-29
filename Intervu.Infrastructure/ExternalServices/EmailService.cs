using System;
using Intervu.Application.Interfaces.ExternalServices;

namespace Intervu.Infrastructure.ExternalServices
{
    public class EmailService : IMailService
    {
        public void SendMail(string to, string subject, string body)
        {
            throw new NotImplementedException();
        }
    }
}
