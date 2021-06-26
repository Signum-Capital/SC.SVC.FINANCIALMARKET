using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SC.FINANCIALMARKET.API.Areas.V1.Controllers
{
    public class CatalogadorController : V1Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Result();
        }
    }
}
