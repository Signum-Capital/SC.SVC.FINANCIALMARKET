using Data.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SC.FINANCIALMARKET.API.Areas.V1.Models;
using SC.FINANCIALMARKET.DOMAIN.Factories;
using SC.INFRA.INFRAESTRUCTURE.DB.FINANCIALMARKETDATA;
using SC.PKG.SERVICES.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SC.FINANCIALMARKET.API.Areas.V1.Controllers
{
    public class CatalogadorController : V1Controller
    {
        private FinancialMarketDataContext FinancialMarketDataContext { get; }

        public CatalogadorController(FinancialMarketDataContext financialMarketDataContext)
        {
            FinancialMarketDataContext = financialMarketDataContext;
        }

        [HttpPost("Catalogar")]
        public IActionResult Post(ConsultaRequest consultaRequest)
        {
            var consulta = ConvertObject.Convert<Consulta>(consultaRequest);
            var factory = new CatalogerFactory(consulta, FinancialMarketDataContext).Produce();
            return Result(factory);
        }
    }
}
