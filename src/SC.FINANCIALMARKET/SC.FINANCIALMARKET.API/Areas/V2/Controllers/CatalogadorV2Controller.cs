using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SC.FINANCIALMARKET.API.Models;
using SC.FINANCIALMARKET.DOMAIN.Factories;
using SC.FINANCIALMARKET.DOMAIN.Hubs;
using SC.FINANCIALMARKET.DOMAIN.Services;
using SC.INFRA.INFRAESTRUCTURE.Contexts;
using SC.INFRA.INFRAESTRUCTURE.DB.FINANCIALMARKETDATA;
using SC.PKG.SERVICES.Services;
using System.Threading;
using System.Threading.Tasks;

namespace SC.FINANCIALMARKET.API.Areas.V2.Controllers
{
    public class CatalogadorV2Controller : V2Controller
    {
        private readonly IHubContext<CatalogadorHub> _hubContext;
        private FinancialMarketDataContext FinancialMarketDataContext { get; }

        public CatalogadorV2Controller(IHubContext<CatalogadorHub> hubContext, FinancialMarketDataContext financialMarketDataContext)
        {
            _hubContext = hubContext;
            FinancialMarketDataContext = financialMarketDataContext;
        }

        [HttpPost("Catalogar/realtime")]
        public async Task<IActionResult> Post(ConsultaRequest consultaRequest)
        {
            var consulta = ConvertObject.Convert<Consulta>(consultaRequest);
            new CatalogerRealTimeFactory(consulta, consultaRequest.ConnectionId, _hubContext).Produce();

            return Result();
        }

        [HttpGet("ForceFinish/{connectionId}")]
        public IActionResult ForceFinishCataloger(string connectionId)
        {
            if (CatalogerService.CancellationQueue.Contains(connectionId))
            {
                CatalogerService.CancellationQueue.Add(connectionId);
            }

            return Ok();
        }

        [HttpGet("checkStatus")]
        public IActionResult CheckStatus()
        {
            return Ok();
        }
    }
}
