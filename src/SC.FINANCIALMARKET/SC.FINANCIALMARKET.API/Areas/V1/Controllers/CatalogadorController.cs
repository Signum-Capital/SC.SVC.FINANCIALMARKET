﻿using Microsoft.AspNetCore.Mvc;
using SC.FINANCIALMARKET.API.Models;
using SC.FINANCIALMARKET.DOMAIN.Factories;
using SC.INFRA.INFRAESTRUCTURE.Contexts;
using SC.INFRA.INFRAESTRUCTURE.DB.FINANCIALMARKETDATA;
using SC.PKG.SERVICES.Models;
using SC.PKG.SERVICES.Services;
using System;
using System.Collections.Generic;
using System.Threading;
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
        public ActionResult<ResponseModel<List<ResultadoItem>>> Post(ConsultaRequest consultaRequest)
        {
            var consulta = ConvertObject.Convert<Consulta>(consultaRequest);
            var factory = new CatalogerFactory(consulta, FinancialMarketDataContext).Produce();
            return ResultAsModel(factory);
        }

        [HttpGet("CollectCandles")]
        public async Task<IActionResult> CollectCandles()
        {
            if (CollectCandleFactory.Running)
                return BadRequest("System is already collecting candles. Try leter.");

            new Thread(() =>
            {
                var collectFactory = new CollectCandleFactory();
                collectFactory.Produce();
            }).Start();

            return Ok($"System started candle collection. {DateTime.Now:dd/MM/yyyy}");
        }
    }
}
