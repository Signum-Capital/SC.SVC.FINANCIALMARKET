using IqOptionApi.Models;
using Microsoft.AspNetCore.SignalR;
using SC.FINANCIALMARKET.DOMAIN.Hubs;
using SC.FINANCIALMARKET.DOMAIN.Models;
using SC.FINANCIALMARKET.DOMAIN.Services;
using SC.INFRA.INFRAESTRUCTURE.Contexts;
using SC.INFRA.INFRAESTRUCTURE.DB.FINANCIALMARKETDATA;
using SC.PKG.SERVICES.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SC.FINANCIALMARKET.DOMAIN.Factories
{
    public class CatalogerRealTimeFactory : Factory<object>
    {
        public Exception Error { get; set; }

        public int RegistrosEncontrados { get; set; }
        public INFRA.INFRAESTRUCTURE.DB.FINANCIALMARKETDATA.Resultado Resultado { get; set; } = new INFRA.INFRAESTRUCTURE.DB.FINANCIALMARKETDATA.Resultado();
        public List<List<List<Candle>>> Candles { get; set; } = new List<List<List<Candle>>>();
        List<DateTime> Horas { get; set; } = new List<DateTime>();
        List<DateTime> Dias { get; set; } = new List<DateTime>();
        int Id = 0;
        bool Canceled { get; set; } = false;


        public IClientProxy ClientProxy { get; }
        public Consulta Consulta { get; }
        public FinancialMarketDataContext FinancialMarketDataContext { get; }
        public string ConnectionId { get; }
        public string[] Paridades { get; set; }

        public CatalogerRealTimeFactory(Consulta consulta, string connectionId, IHubContext<CatalogadorHub> hubContext)
        {
            Consulta = consulta;
            FinancialMarketDataContext = new FinancialMarketDataContext();
            ConnectionId = connectionId;
            ClientProxy = hubContext.Clients.Client(connectionId);
        }

        public async override Task<object> ProduceAsync()
        {
            Paridades = Consulta.Paridades.ToUpper().Replace(" ", "").Split(",");

            Query().ConfigureAwait(false);
            return Resultado.ResultadoItens.ToList();
        }

        /// <summary>
        /// Código não está devidamente padronizado no pattern pois foi extraido da plataforma anterior.
        /// </summary>
        private async Task Query()
        {
            if (Paridades.Length > 0)
            {
                Horas = CarregaHoras(Consulta.TimeFrame);
                Dias = CarregaDias(Consulta.TotalDias, Paridades);
                var ListaCompleta = new List<ResultadoItem>();

                foreach (var paridade in Paridades.ToList())
                {
                    try
                    {
                        Dictionary<DateTime, List<List<Candle>>> SinaisHorarios = CarregaSinaisHorarios(Horas, Dias, paridade);
                        Dictionary<DateTime, Ordem> PorcentagemOrdem = CarregaOrdemPorcentagem(SinaisHorarios);
                        var listaAAdicionar = CarregarResultadoItens(SinaisHorarios, PorcentagemOrdem, paridade);

                        if (Canceled)
                            break;
                    }
                    catch (Exception e)
                    {
                        await ClientProxy.SendAsync("RecieveResult", "ERROR: " + e.Message);
                    }
                }

                await ClientProxy.SendAsync("RecieveResult", ConnectionId, "FINISH");

                Resultado.ResultadoItens = ListaCompleta;
            }
        }

        private ICollection<ResultadoItem> CarregarResultadoItens(Dictionary<DateTime, List<List<Candle>>> sinaisHorarios, Dictionary<DateTime, Ordem> porcentagemOrdem, string paridade)
        {
            var result = new List<ResultadoItem>();

            if (sinaisHorarios != null)
            {
                ClientProxy.SendAsync("RecieveResult", ConnectionId, "{\"START\": { \"MAX_ITEMS\": " + sinaisHorarios.Count + ", \"PARIDADE\": \"" + paridade + "\"} }");

                foreach (var sinais in sinaisHorarios)
                {
                    if (CatalogerService.CancellationQueue.Any(e => e == ConnectionId))
                    {
                        CatalogerService.CancellationQueue.Remove(ConnectionId);
                        Canceled = true;
                        break;
                    }

                    var ordem = porcentagemOrdem[sinais.Key];
                    var stringOrdem = ordem.OrdemCandle.HasValue ? ordem.OrdemCandle == OrderDirection.Put ? "VENDA" : "COMPRA" : "NEUTRO";

                    var valido = false;
                    var loss = 0;

                    foreach (var item in sinais.Value)
                    {
                        if(ordem.Porcentagem < Consulta.PorcentagemVelas)
                        {
                            valido = false;
                            break;
                        }

                        foreach (var subitem in item)
                        {
                            OrderDirection? ordemDoCandle = subitem.Abertura > subitem.Fechamento ? OrderDirection.Put : subitem.Abertura == subitem.Fechamento ? null : OrderDirection.Call;
                            if (ordemDoCandle == ordem.OrdemCandle)
                            {
                                valido = true;
                                break;
                            }
                            else
                            {
                                valido = false;
                            }
                        }

                        if (valido)
                             continue;
                        else
                            loss++;

                        if (loss > Consulta.TotalLoss)
                        {
                            valido = false;
                            break;
                        }
                    }

                    if (valido)
                    {
                        var itemValido = new ResultadoItem()
                        {
                            Id = Id++,
                            DateTime = sinais.Key.AddDays(1).AddHours(Consulta.Timezone),
                            Ordem = stringOrdem,
                            Paridade = paridade,
                            Porcentagem = ordem.Porcentagem,
                            Timeframe = Consulta.TimeFrame,
                            Gales = Consulta.Gale,
                            ResultadoCandles = GetResultadoCandle(sinais.Value)
                        };

                        ClientProxy.SendAsync("RecieveResult", ConnectionId, JsonSerializer.Serialize(itemValido));
                    }
                }
            }

            return result;
        }

        private ICollection<ResultadoItem> ValidarRemoverHorarios(ICollection<ResultadoItem> resultadoItens)
        {
            foreach (var item in resultadoItens.ToList())
            {
                if (item.Porcentagem < Consulta.PorcentagemVelas)
                {
                    resultadoItens.Remove(item);
                    continue;
                }
            }

            return resultadoItens;
        }

        private Dictionary<DateTime, Ordem> CarregaOrdemPorcentagem(Dictionary<DateTime, List<List<Candle>>> sinaisHorarios)
        {
            var result = new Dictionary<DateTime, Ordem>();

            if (sinaisHorarios != null)
            {
                foreach (var item in sinaisHorarios)
                {
                    var getPorcentagemCompra = GetPorcentagemCompra(item.Value);
                    var ordem = GetOrdem(item.Value);
                    double porcentagem = ordem.HasValue ? ordem == OrderDirection.Put ? 100 - getPorcentagemCompra : getPorcentagemCompra : 50;

                    result.Add(item.Key, new Ordem(porcentagem, ordem));
                }
            }

            return result;
        }

        private Dictionary<DateTime, List<List<Candle>>> CarregaSinaisHorarios(List<DateTime> horas, List<DateTime> dias, string paridade)
        {
            if (dias.Count > 0)
            {
                var primeiroDia = dias.First().Date;
                var ultimoDia = dias.Last();
                var listAllCandles = FinancialMarketDataContext.Candles.Where(e => e.Data.Date >= ultimoDia && e.Data.Date <= primeiroDia && e.Paridade == paridade && e.Timeframe == Consulta.TimeFrame).ToList();
                var result = new Dictionary<DateTime, List<List<Candle>>>();

                foreach (var hora in horas)
                {
                    var candleDias = new List<List<Candle>>();
                    var msmHorario = listAllCandles.Where(e => e.Data.Hour == hora.Hour && e.Data.Minute == hora.Minute && e.Paridade == paridade).TakeLast(Consulta.TotalDias).ToList();

                    foreach (var dia in msmHorario)
                    {
                        var candleGales = listAllCandles.Where(e => e.Data >= dia.Data && e.Data < dia.Data.AddMinutes(Consulta.TimeFrame * (Consulta.Gale + 1))).ToList();

                        if (candleGales.Count > Consulta.Gale + 1)
                            candleGales = candleGales.GroupBy(e => e.Data).Select(e => e.First()).ToList();

                        candleDias.Add(candleGales);
                    }

                    var valid = Consulta.Tendencia switch
                    {
                        5 => candleDias[0][0].Tendencia5,
                        10 => candleDias[0][0].Tendencia10,
                        15 => candleDias[0][0].Tendencia15,
                        30 => candleDias[0][0].Tendencia30,
                        _ => true
                    };

                    if (valid)
                        result.Add(hora, candleDias);
                }

                return result;
            }

            return null;
        }

        private List<DateTime> CarregaDias(int totalDias, string[] paridades)
        {
            var list = new List<DateTime>();
            var diaAtual = DateTime.UtcNow.AddDays(-1);

            while (list.Count <= totalDias + 3)
            {
                if (FinancialMarketDataContext.Candles.Any(e => e.Data.Date == diaAtual.Date && paridades.Contains(e.Paridade)))
                    list.Add(diaAtual.Date);

                diaAtual = diaAtual.AddDays(-1);

                if ((DateTime.UtcNow.Date - diaAtual.Date).TotalDays > Consulta.TotalDias * 3)
                    break;
            }

            return list;
        }

        private List<DateTime> CarregaHoras(int timeframe)
        {
            var list = new List<DateTime>();
            var comp = new DateTime(1, 1, 1, 0, 0, 0);
            while (comp < new DateTime(1, 1, 1, 23, 59, 59))
            {
                list.Add(comp);
                comp = comp.AddMinutes(timeframe);
            }

            return list;
        }

        private OrderDirection? GetOrdem(List<List<Candle>> getResultado)
        {
            var porcentagemDeCompra = GetPorcentagemCompra(getResultado);

            if (porcentagemDeCompra > 50)
            {
                return OrderDirection.Call;
            }
            else if (porcentagemDeCompra < 50)
            {
                return OrderDirection.Put;
            }
            else
            {
                return null;
            }
        }

        private string GetResultadoCandle(List<List<Candle>> candles)
        {
            var list = candles.Select(e => e.OrderBy(e => e.Data).Select(x => new Item(x, Consulta.Timezone)).ToList()).ToList();
            list.ForEach(e => e.ForEach(x => x.Candle.Data = x.Candle.Data));//.AddHours(_consulta.Timezone)

            var json = JsonSerializer.Serialize(list);
            return json;
        }

        private double GetPorcentagemCompra(List<List<Candle>> candles)
        {
            var list = new List<Candle>();
            candles.ForEach(e => e.ForEach(x => list.Add(x)));
            return Convert.ToDouble(list.Where(e => e.Abertura < e.Fechamento).Count()) / Convert.ToDouble(list.Count) * 100;
        }

        public void SalvarConfiguracao(int salaId)
        {
            if (!FinancialMarketDataContext.Consultas.Any(e => e.Gale == Consulta.Gale &&
                                             e.Paridades == Consulta.Paridades &&
                                             e.PorcentagemVelas == Consulta.PorcentagemVelas &&
                                             e.Tendencia == Consulta.Tendencia &&
                                             e.TimeFrame == Consulta.TimeFrame &&
                                             e.TotalDias == Consulta.TotalDias &&
                                             e.TotalLoss == Consulta.TotalLoss &&
                                             e.SalaId == salaId))
            {
                Consulta.SalaId = salaId;
                FinancialMarketDataContext.Consultas.Add(Consulta);
                FinancialMarketDataContext.SaveChanges();
            }
        }

    }
}
