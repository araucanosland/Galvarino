using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Services.Notification
{
    public interface INotificationKernel
    {
        void Send(string to, string template);
        Task SendEmail(string email, string subject, string message);
    }
}
