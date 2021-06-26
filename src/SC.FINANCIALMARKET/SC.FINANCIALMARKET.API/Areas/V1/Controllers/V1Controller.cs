using Microsoft.AspNetCore.Mvc;
using SC.FINANCIALMARKET.API.Controllers.Generic;

namespace SC.FINANCIALMARKET.API.Areas.V1.Controllers
{
    [ApiController]
    [Route("api/v1/[Controller]")]
    public abstract class V1Controller : GenericController { }
}
