using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;


using SC.FINANCIALMARKET.DOMAIN.Factories;
using SC.FINANCIALMARKET.DOMAIN.Hubs;
using SC.FINANCIALMARKET.DOMAIN.Models;
using SC.FINANCIALMARKET.DOMAIN.Services;
using SC.INFRA.INFRAESTRUCTURE.Contexts;
using SC.INFRA.INFRAESTRUCTURE.DB.FINANCIALMARKETDATA;
using SC.INFRA.INFRAESTRUCTURE.Interfaces;
using SC.PKG.SERVICES.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SC.FINANCIALMARKET.API.Areas.V2.Controllers
{
    public class CatalogadorV2Controller : V2Controller
    {
        private readonly IHubContext<CatalogadorHub> _hubContext;
        private FinancialMarketDataContext FinancialMarketDataContext { get; }
        public IUsuarioPlataformaRepository UsuarioPlataformaRepository { get; }

        public CatalogadorV2Controller(IHubContext<CatalogadorHub> hubContext, FinancialMarketDataContext financialMarketDataContext, IUsuarioPlataformaRepository usuarioPlataformaRepository)
        {
            _hubContext = hubContext;
            FinancialMarketDataContext = financialMarketDataContext;
            UsuarioPlataformaRepository = usuarioPlataformaRepository;
        }

        [HttpPost("Catalogar/realtime")]
        public async Task<IActionResult> Post(ConsultaRequest consultaRequest)
        {
            var usuplat = await UsuarioPlataformaRepository.RecuperarPorUsuarioPlataformaAsync(Token.UsuarioId, 1009);

            if (usuplat == null)
                return Result(null, "USER_NOT_FOUND", false);

            //else if (usuplat.Expiracao < DateTime.UtcNow)
            //    return Result(null,"TIME_EXPIRED",false);

            var consulta = consultaRequest;
            consulta.Data = DateTime.Now;
            
            consulta.Timezone = -3;

            new CatalogerRealTimeFactory(consulta, consultaRequest.ConnectionId, _hubContext).Produce();

            return Result();
        }

        [HttpGet("ForceFinish/{connectionId}")]
        public IActionResult ForceFinishCataloger(string connectionId)
        {
            if (!CatalogerService.CancellationQueue.Contains(connectionId))
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
