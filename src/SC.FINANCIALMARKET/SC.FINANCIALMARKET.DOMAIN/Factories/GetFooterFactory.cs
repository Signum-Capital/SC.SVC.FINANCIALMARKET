using SC.FINANCIALMARKET.DOMAIN.Models;
using SC.PKG.SERVICES.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC.FINANCIALMARKET.DOMAIN.Factories
{
    public class GetFooterFactory : Factory<ListWithFooter>
    {

        public List<ResultadoSinal> ListaResultado { get; set; }

        public GetFooterFactory(List<ResultadoSinal> listaResultado)
        {
            ListaResultado = listaResultado;
        }

        public override async Task<ListWithFooter> ProduceAsync()
        {
            //Total: 0 //Total de sinais enviados
            //Apurado: 0 //Total de sinais que ja foram finalizados
            //WinPrimeira: 0 //Total de win de primeira
            //PercentualWinPrimeira: 0 //% do total de win de primeira
            //WinGale: 0 //Total de win com gale (independente de quantos gales)
            //PercentualWinGale: 0 //% do total de win com gale
            //Assertividade: 0 //% de sinais que finalizaram com win

            var Total = ListaResultado.Count();
            var Apurado = ListaResultado.Count(e => e.Resultado != "Aguardando...");
            var WinPrimeira = ListaResultado.Count(e => e.Resultado == "WIN");
            var WinGale = ListaResultado.Count(e => e.Resultado != "WIN" && e.Resultado != "LOSS" && e.Resultado != "Aguardando...");
            var PercentualWinPrimeira = (WinPrimeira * 100) / Apurado;
            var PercentualWinGale = (WinGale * 100) / Apurado;
            var Assertividade = ((WinGale + WinPrimeira) * 100) / Apurado;

            var listaRodape = new ListWithFooter
            {
                Lista = ListaResultado,
                Footer = new Footer(Total,Apurado,WinPrimeira,WinGale,PercentualWinPrimeira,PercentualWinGale,Assertividade)
            };

            return listaRodape;
        }


    }
}
