using Microsoft.AspNetCore.Mvc;
using SC.FINANCIALMARKET.DOMAIN.Factories;
using SC.FINANCIALMARKET.DOMAIN.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SC.FINANCIALMARKET.API.Areas.V2.Controllers
{
    public class CheckListController : V2Controller
    {
        public CheckListController()
        {

        }

        [HttpPost]
        public IActionResult CheckList(EntradaSinal entradaSinal)
        {
            try
            {
                //Padronizar a lista//
                //Transformar no objeto ResultadoSinal//                        
                var listaTratada = new PatternizedListFactory(entradaSinal).Produce();

                //Pega o resultados dos candles da lista//
                var listaResultados = new CollectResultCandleFactory(listaTratada).Produce();

                //Retorna lista//
                return Ok(listaResultados);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        [HttpPost("WithFooter")]
        public IActionResult CheckListWithFooter(EntradaSinal entradaSinal)
        {
            try
            {
                //Padronizar a lista//
                //Transformar no objeto ResultadoSinal//                        
                var listaTratada = new PatternizedListFactory(entradaSinal).Produce();

                //Pega o resultados dos candles da lista//
                var listaResultados = new CollectResultCandleFactory(listaTratada).Produce();

                //Retorna o rodapé da lista
                var listaRodape = new GetFooterFactory(listaResultados).Produce();

                //Retorna lista
                return Ok(listaRodape);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }
    }
}
