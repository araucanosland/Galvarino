using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Services.Notification
{
    public interface INotificationKernel
    {
        void EnviaMail();

        void EnviaPush();
    }
}
