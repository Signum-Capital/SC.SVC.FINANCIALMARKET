using KissLog;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SC.FINANCIALMARKET.API.Areas.V1.Controllers
{
    public class Teste : V1Controller
    {
        ILogger _logger { get; }

        public Teste(ILogger logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult teste()
        {
            _logger.Debug("Aplicação testada");
            return Ok();
        }
    }
}
