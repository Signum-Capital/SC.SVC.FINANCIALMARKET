using Microsoft.AspNetCore.Mvc;
using SC.FINANCIALMARKET.API.Controllers.Generic;

namespace SC.FINANCIALMARKET.API.Areas.V2.Controllers
{
    [ApiController]
    [Route("api/v2/[Controller]")]
    public abstract class V2Controller : GenericController { }
}
