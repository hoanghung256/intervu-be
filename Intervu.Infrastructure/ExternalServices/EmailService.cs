using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.ExternalServices;

namespace Intervu.Infrastructure.Services
{
    public class EmailService : IMailService
    {
        public void SendMail(string to, string subject, string body)
        {
            throw new NotImplementedException();
        }
    }
}
