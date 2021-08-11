using IqOptionApi.Models;
using SC.FINANCIALMARKET.DOMAIN.Configuration;
using SC.FINANCIALMARKET.DOMAIN.Models;
using SC.PKG.SERVICES.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC.FINANCIALMARKET.DOMAIN.Factories
{
    public class CollectResultCandleFactory : Factory<List<ResultadoSinal>>
    {
        public List<ResultadoSinal> Entrada { get; set; }

        IqOptionApi.IqOptionClient IqOptionClient { get; }


        //Receber Lista
        //Buscar sinais na IQ e trazer o resultado
        //tratar gales?
        //retornar lista
        public CollectResultCandleFactory(List<ResultadoSinal> entrada)
        {
            Entrada = entrada;
            IqOptionClient = SigninIqOptionConfiguration.IqOptionClient;
        }


        public override async Task<List<ResultadoSinal>> ProduceAsync()
        {
            foreach (var sinal in Entrada)
            {
                var sinalAtivo = getActivePair(sinal.Paridade.Replace("-", "_"));
                var timeframe = (TimeFrame)(sinal.Timeframe * 60);
                var qtd = 1 + sinal.Gale;

                //var horario = sinal.Horario.ToUniversalTime();
                //var candles = await IqOptionClient.GetCandlesAsync(sinalAtivo, timeframe, qtd, horario);

                var horario = sinal.Horario.AddMinutes(sinal.Gale * sinal.Timeframe).ToUniversalTime();

                var candles = IqOptionClient.GetCandlesAsync(sinalAtivo, timeframe, qtd, horario).GetAwaiter().GetResult();

                int countLose = 0;
                foreach (var candle in candles.Infos)
                {
                    if (horario > DateTime.UtcNow)
                    {
                        sinal.Resultado = "Aguardando...";
                        break;
                    }

                    var doji = candle.Close == candle.Open;

                    if (doji)
                    {
                        sinal.Resultado = "DOJI";
                        break;
                    }

                    var win = (candle.Open > candle.Close) && sinal.Ordem.ToUpper() == "PUT" ||
                              (candle.Open < candle.Close) && sinal.Ordem.ToUpper() == "CALL";

                    if (win)
                    {
                        sinal.Resultado = (countLose == 0 ? "WIN" : countLose + " GALE");
                        break;
                    }
                    else
                    {
                        countLose++;
                    }

                    if (candle == candles.Infos.Last())
                    {
                        sinal.Resultado = "LOSS";
                    }

                }
            }

            var listaCompleta = Entrada;

            return listaCompleta;

        }


        private ActivePair getActivePair(string paridade)
        {
            ActivePair active = (ActivePair)System.Enum.Parse(typeof(ActivePair), paridade);

            return active;
        }



    }
}
