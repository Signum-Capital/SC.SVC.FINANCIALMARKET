using IqOptionApi;
using IqOptionApi.Models;
using SC.INFRA.INFRAESTRUCTURE.Contexts;
using SC.INFRA.INFRAESTRUCTURE.DB.FINANCIALMARKETDATA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SC.FINANCIALMARKET.DOMAIN.Services
{
    public class CapturaCandle
    {
        public DateTime DiaVigente { get; set; }

        public List<ActivePair> Ativos { get; internal set; }
        public FinancialMarketDataContext Context { get; internal set; }
        public List<TimeFrame> Timeframes { get; internal set; }
        public IqOptionClient IqClient { get; internal set; }

        public CapturaCandle(DateTime diaVigente)
        {
            DiaVigente = diaVigente;
        }

        public List<Candle> SaveCandles()
        {
            var result = new List<Candle>();

            if (Context.Candles.Any(e => e.Data.Date == DiaVigente.Date && e.Data.Hour >= 20))
            {
                return result;
            }

            var count = 0;
            foreach (var Ativo in Ativos)
            {
                foreach (var Timeframe in Timeframes)
                {
                    int TotalMinutos = Convert.ToInt32((DiaVigente.AddDays(1) - DiaVigente).TotalMinutes);
                    int TotalCandles = Convert.ToInt32(TotalMinutos / ((int)Timeframe / 60));

                    List<Candle> ListaParaAdicionar = new List<Candle>();

                    if (Timeframe == TimeFrame.Min1)
                    {
                        var candleM1Ate12hrs = IqClient.GetCandlesAsync(Ativo, Timeframe, TotalCandles, DiaVigente.AddHours(12).AddSeconds(-1)).ConfigureAwait(true).GetAwaiter().GetResult();
                        var candlesM1Tratados = candleM1Ate12hrs.Infos.Select(e => new Candle()
                        {
                            IdIqoption = e.Id,
                            Abertura = e.Open,
                            Fechamento = e.Close,
                            Maximo = e.Max,
                            Minimo = e.Min,
                            Paridade = Ativo.ToString(),
                            Data = e.From.UtcDateTime,
                            Timeframe = (int)Timeframe / 60
                        }).ToList();

                        candlesM1Tratados = candlesM1Tratados.Where(e => e.Data.Date == DiaVigente.Date).ToList();
                        TratarTendencias(candlesM1Tratados);
                        ListaParaAdicionar.AddRange(candlesM1Tratados);
                    }

                    var candles = IqClient.GetCandlesAsync(Ativo, Timeframe, TotalCandles, DiaVigente.AddDays(1).AddSeconds(-1)).ConfigureAwait(true).GetAwaiter().GetResult();

                    var candlesTratados = candles.Infos.Select(e => new Candle()
                    {
                        IdIqoption = e.Id,
                        Abertura = e.Open,
                        Fechamento = e.Close,
                        Maximo = e.Max,
                        Minimo = e.Min,
                        Paridade = Ativo.ToString(),
                        Data = e.From.UtcDateTime,
                        Timeframe = (int)Timeframe / 60
                    }).ToList();

                    candlesTratados = candlesTratados.Where(e => e.Data.Date == DiaVigente.Date).ToList();
                    TratarTendencias(candlesTratados);
                    ListaParaAdicionar.AddRange(candlesTratados);

                    ListaParaAdicionar = LimparDuplicados(ListaParaAdicionar);
                    Context.Candles.AddRange(ListaParaAdicionar);
                }
            }

            Context.SaveChanges();
            return result;
        }

        private List<Candle> LimparDuplicados(List<Candle> listaParaAdicionar)
        {
            var result = new List<Candle>();

            var groups = listaParaAdicionar
               .GroupBy(e => new { e.Data, e.Paridade, e.Timeframe });

            groups.ToList().ForEach(item => result.Add(item.First()));
            return result;
        }

        public void FormatarAPartirDe(DateTime date)
        {
            if (DiaVigente.Date >= date.Date)
            {
                var candles = Context.Candles.Where(e => e.Data.Date >= date).ToList();
                candles.ForEach(e => Context.Candles.Remove(e));
                Context.SaveChanges();
            }
        }

        public void ApagarDias()
        {
            var candles = Context.Candles.Where(e => e.Data.Date >= DateTime.UtcNow.AddDays(-40)).ToList();
            candles.ForEach(e => Context.Candles.Remove(e));
            Context.SaveChanges();
        }

        public void LimparAntigos(DateTime dateTime)
        {
            CalcularTempo("A limpeza demorou: ", () =>
            {
                var listDelete = Context.Candles.Where(e => e.Data < dateTime);
                Context.Candles.RemoveRange(listDelete);
                Context.SaveChanges();
            });
        }

        public void LimparDuplicados()
        {
            var groups = Context.Candles
                .Where(e => e.Data.Date > DiaVigente.Date.AddDays(-7))
                .ToList()
                .GroupBy(e => new { e.Data, e.Paridade, e.Timeframe });

            groups.ToList().ForEach(item => Context.Candles.RemoveRange(item.Skip(1)));
            Context.SaveChanges();
        }

        private void TratarTendencias(List<Candle> candlesTratados)
        {
            for (int i = 0; i < candlesTratados.Count; i++)
            {
                if (i > 6)
                {
                    var candle = candlesTratados[i];
                    var candleTendencia = candlesTratados[i - 6];

                    var tendencia = candleTendencia.Abertura > candlesTratados[i - 1].Fechamento && candle.Abertura > candle.Fechamento ||
                                    candleTendencia.Abertura < candlesTratados[i - 1].Fechamento && candle.Abertura < candle.Fechamento;

                    candle.Tendencia5 = tendencia;
                }

                if (i > 11)
                {
                    var candle = candlesTratados[i];
                    var candleTendencia = candlesTratados[i - 11];

                    var tendencia = candleTendencia.Abertura > candlesTratados[i - 1].Fechamento && candle.Abertura > candle.Fechamento ||
                                    candleTendencia.Abertura < candlesTratados[i - 1].Fechamento && candle.Abertura < candle.Fechamento;

                    candle.Tendencia10 = tendencia;
                }

                if (i > 16)
                {
                    var candle = candlesTratados[i];
                    var candleTendencia = candlesTratados[i - 16];

                    var tendencia = candleTendencia.Abertura > candlesTratados[i - 1].Fechamento && candle.Abertura > candle.Fechamento ||
                                    candleTendencia.Abertura < candlesTratados[i - 1].Fechamento && candle.Abertura < candle.Fechamento;

                    candle.Tendencia15 = tendencia;
                }

                if (i > 31)
                {
                    var candle = candlesTratados[i];
                    var candleTendencia = candlesTratados[i - 31];

                    var tendencia = candleTendencia.Abertura > candlesTratados[i - 1].Fechamento && candle.Abertura > candle.Fechamento ||
                                    candleTendencia.Abertura < candlesTratados[i - 1].Fechamento && candle.Abertura < candle.Fechamento;

                    candle.Tendencia30 = tendencia;
                }
            }
        }

        static void CalcularTempo(string title, Action action)
        {
            DateTime inicio = DateTime.Now;
            action.Invoke();
            DateTime fim = DateTime.Now;
            Console.WriteLine(title + ": " + (fim - inicio).TotalSeconds + "s");
        }
    }
}
