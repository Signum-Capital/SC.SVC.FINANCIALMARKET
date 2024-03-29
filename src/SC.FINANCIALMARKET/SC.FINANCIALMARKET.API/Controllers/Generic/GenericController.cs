﻿using Microsoft.AspNetCore.Mvc;
using SC.FINANCIALMARKET.API.Models;
using SC.FINANCIALMARKET.DOMAIN.Collections;
using SC.INFRA.INFRAESTRUCTURE.DB.SIGNUMCAPITAL;
using SC.PKG.SERVICES.Models;
using SC.PKG.SERVICES.Services;

namespace SC.FINANCIALMARKET.API.Controllers.Generic
{
    public abstract class GenericController : ControllerBase
    {
        protected Plataforma CurrentPlataform { get => HttpContext.Items.ContainsKey(GlobalInfo.PlataformName) ? (Plataforma)HttpContext.Items[GlobalInfo.PlataformName] : default; }

        protected IActionResult Result(object data = null, string message = "", bool isOk = true)
        {
            var res = new ResultViewModel
            {
                Data = data,
                Message = message
            };

            return isOk ? Ok(res) : BadRequest(res);
        }

        protected ActionResult<ResponseModel<T>> ResultAsModel<T>(T data, string message = "", bool isOk = true)
        {
            var res = new ResultViewModel
            {
                Data = data,
                Message = message
            };

            return isOk ? Ok(res) : BadRequest(res);
        }

        protected Token Token
        {
            get
            {
                var token = HeaderService.GetAuthorization(Request);

                try
                {
                    var user = TokenService.RevertTokenJwt(token);
                    return user;
                }
                catch
                {
                    var user = TokenService.RevertToken(token);
                    return user;
                }
            }
        }
    }
}
