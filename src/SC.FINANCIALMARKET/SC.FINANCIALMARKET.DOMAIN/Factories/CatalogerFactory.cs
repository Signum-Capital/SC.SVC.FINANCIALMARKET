using IqOptionApi.Models;
using SC.FINANCIALMARKET.DOMAIN.Models;
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
    [Obsolete("Factory descontinuada para utilização da CatalogerRealTimeFactory")]
    public class CatalogerFactory : Factory<List<ResultadoItem>>
    {
        public Exception Error { get; set; }

        public int RegistrosEncontrados { get; set; }
        public INFRA.INFRAESTRUCTURE.DB.FINANCIALMARKETDATA.Resultado Resultado { get; set; } = new INFRA.INFRAESTRUCTURE.DB.FINANCIALMARKETDATA.Resultado();
        public List<List<List<Candle>>> Candles { get; set; } = new List<List<List<Candle>>>();
        List<DateTime> Horas { get; set; } = new List<DateTime>();
        List<DateTime> Dias { get; set; } = new List<DateTime>();
        int Id = 0;


        Consulta _consulta { get; set; }
        FinancialMarketDataContext _context { get; set; }


        public CatalogerFactory(Consulta consulta, FinancialMarketDataContext context)
        {
            _consulta = consulta;
            _context = context;
        }

        public async override Task<List<ResultadoItem>> ProduceAsync()
        {
            Query();
            return Resultado.ResultadoItens.ToList();
        }

        /// <summary>
        /// Código não está devidamente padronizado no pattern pois foi extraido da plataforma anterior.
        /// </summary>
        private void Query()
        {
            var Paridades = _consulta.Paridades.ToUpper().Replace(" ", "").Split(",");

            if (Paridades.Length > 0)
            {
                Horas = CarregaHoras(_consulta.TimeFrame);
                Dias = CarregaDias(_consulta.TotalDias, Paridades);
                var ListaCompleta = new List<ResultadoItem>();
                foreach (var paridade in Paridades.ToList())
                {
                    Dictionary<DateTime, List<List<Candle>>> SinaisHorarios = CarregaSinaisHorarios(Horas, Dias, paridade);
                    Dictionary<DateTime, Ordem> PorcentagemOrdem = CarregaOrdemPorcentagem(SinaisHorarios);
                    var listaAAdicionar = CarregarResultadoItens(SinaisHorarios, PorcentagemOrdem, paridade);
                    ListaCompleta.AddRange(ValidarRemoverHorarios(listaAAdicionar));
                }

                Resultado.ResultadoItens = ListaCompleta;
            }
        }

        private ICollection<ResultadoItem> CarregarResultadoItens(Dictionary<DateTime, List<List<Candle>>> sinaisHorarios, Dictionary<DateTime, Ordem> porcentagemOrdem, string paridade)
        {
            var result = new List<ResultadoItem>();

            foreach (var sinais in sinaisHorarios)
            {
                var ordem = porcentagemOrdem[sinais.Key];
                var stringOrdem = ordem.OrdemCandle.HasValue ? ordem.OrdemCandle == OrderDirection.Put ? "VENDA" : "COMPRA" : "NEUTRO";

                var valido = false;
                var loss = 0;

                foreach (var item in sinais.Value)
                {
                    foreach (var subitem in item)
                    {
                        OrderDirection? ordemDoCandle = subitem.Abertura > subitem.Fechamento ? OrderDirection.Put : subitem.Abertura == subitem.Fechamento ? null : OrderDirection.Call;
                        if (ordemDoCandle == ordem.OrdemCandle)
                        {
                            valido = true;
                            break;
                        }
                    }

                    if (valido)
                        break;
                    else
                        loss++;

                    if (loss > _consulta.TotalLoss)
                    {
                        valido = false;
                        break;
                    }
                }

                if (valido)
                {
                    result.Add(new ResultadoItem()
                    {
                        Id = Id++,
                        DateTime = sinais.Key.AddDays(1).AddHours(_consulta.Timezone),
                        Ordem = stringOrdem,
                        Paridade = paridade,
                        Porcentagem = ordem.Porcentagem,
                        Timeframe = _consulta.TimeFrame,
                        Gales = _consulta.Gale,
                        ResultadoCandles = GetResultadoCandle(sinais.Value)
                    });
                }
            }

            return result;
        }

        private ICollection<ResultadoItem> ValidarRemoverHorarios(ICollection<ResultadoItem> resultadoItens)
        {
            foreach (var item in resultadoItens.ToList())
            {
                if (item.Porcentagem < _consulta.PorcentagemVelas)
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

            foreach (var item in sinaisHorarios)
            {
                var getPorcentagemCompra = GetPorcentagemCompra(item.Value);
                var ordem = GetOrdem(item.Value);
                double porcentagem = ordem.HasValue ? ordem == OrderDirection.Put ? 100 - getPorcentagemCompra : getPorcentagemCompra : 50;

                result.Add(item.Key, new Ordem(porcentagem, ordem));
            }

            return result;
        }

        private Dictionary<DateTime, List<List<Candle>>> CarregaSinaisHorarios(List<DateTime> horas, List<DateTime> dias, string paridade)
        {
            var primeiroDia = dias.First().Date;
            var ultimoDia = dias.Last();
            var listAllCandles = _context.Candles.Where(e => e.Data.Date >= ultimoDia && e.Data.Date <= primeiroDia && e.Paridade == paridade && e.Timeframe == _consulta.TimeFrame).ToList();
            var result = new Dictionary<DateTime, List<List<Candle>>>();

            foreach (var hora in horas)
            {
                var listGales = new List<List<Candle>>();
                var msmHorario = listAllCandles.Where(e => e.Data.Hour == hora.Hour && e.Data.Minute == hora.Minute && e.Paridade == paridade).ToList();

                foreach (var dia in msmHorario)
                {
                    var add = listAllCandles.Where(e => e.Data >= dia.Data && e.Data < dia.Data.AddMinutes(_consulta.TimeFrame * (_consulta.Gale + 1))).ToList();

                    if (add.Count > _consulta.Gale + 1)
                        add = add.GroupBy(e => e.Data).Select(e => e.First()).ToList();

                    listGales.Add(add);
                }

                var valid = _consulta.Tendencia switch
                {
                    5 => listGales[0][0].Tendencia5,
                    10 => listGales[0][0].Tendencia10,
                    15 => listGales[0][0].Tendencia15,
                    30 => listGales[0][0].Tendencia30,
                    _ => true
                };

                if (valid)
                    result.Add(hora, listGales);
            }

            return result;
        }

        private List<DateTime> CarregaDias(int totalDias, string[] paridades)
        {
            var list = new List<DateTime>();
            var diaAtual = DateTime.UtcNow.AddDays(-1);

            while (list.Count <= totalDias - 1)
            {
                if (_context.Candles.Any(e => e.Data.Date == diaAtual.Date && paridades.Contains(e.Paridade)))
                    list.Add(diaAtual.Date);

                diaAtual = diaAtual.AddDays(-1);
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
            var list = candles.Select(e => e.OrderBy(e => e.Data).Select(x => new Item(x, _consulta.Timezone)).ToList()).ToList();
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
            if (!_context.Consultas.Any(e => e.Gale == _consulta.Gale &&
                                             e.Paridades == _consulta.Paridades &&
                                             e.PorcentagemVelas == _consulta.PorcentagemVelas &&
                                             e.Tendencia == _consulta.Tendencia &&
                                             e.TimeFrame == _consulta.TimeFrame &&
                                             e.TotalDias == _consulta.TotalDias &&
                                             e.TotalLoss == _consulta.TotalLoss &&
                                             e.SalaId == salaId))
            {
                _consulta.SalaId = salaId;
                _context.Consultas.Add(_consulta);
                _context.SaveChanges();
            }
        }

        
    }
}
