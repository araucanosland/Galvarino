using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;


namespace Galvarino.Web.Services.Notification
{
    public class PushHub : Hub
    {

        public async Task SendMessage(string user, string message)
        { 
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        

        public async Task NotificarCotizacionEntrante(string identificadorUsuario, string numeroTicket)
        {
            await Clients.User(identificadorUsuario).SendAsync("NotificarCotizacionEntrante", numeroTicket);
        }

        public async Task NotificarCotizacionSaliente(string identificadorUsuario, string numeroTicket)
        {
            await Clients.User(identificadorUsuario).SendAsync("NotificarCotizacionSaliente", numeroTicket);
        }
    }
}
