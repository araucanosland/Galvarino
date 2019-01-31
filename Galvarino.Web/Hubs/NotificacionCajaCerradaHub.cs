using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Hubs
{
    public class NotificacionCajaCerradaHub : Hub
    {
        public async Task NotificarCajaCerrada(string user, string codigo)
        {
            await Clients.User(user).SendAsync("NotificarCajaCerrada", codigo);
        }

    }
}
