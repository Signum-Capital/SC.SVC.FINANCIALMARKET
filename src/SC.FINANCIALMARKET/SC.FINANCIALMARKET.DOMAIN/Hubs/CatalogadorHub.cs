using Microsoft.AspNetCore.SignalR;
using SC.INFRA.INFRAESTRUCTURE.DB.FINANCIALMARKETDATA;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace SC.FINANCIALMARKET.DOMAIN.Hubs
{
    public class CatalogadorHub : Hub
    {
        public async Task SendResultItem(string user, ResultadoItem message)
        {
            await Clients.All.SendAsync("RecieveResult", user, JsonSerializer.Serialize(message));
        }
    }
}
