using IqOptionApi.Models;
using SC.FINANCIALMARKET.DOMAIN.Services;
using SC.INFRA.INFRAESTRUCTURE.Contexts;
using SC.PKG.SERVICES.Factory;
using SC.PKG.SERVICES.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SC.FINANCIALMARKET.DOMAIN.Factories
{
    public class CollectCandleFactory : Factory<bool>
    {
        public static bool Running { get; set; }

        public List<ActivePair> Ativos { get => Collections.ValidActivesPairs.GetActivePairs; }
        public List<TimeFrame> Timeframes { get => Collections.ValidTimesFrame.TimeFrames; }
        public FinancialMarketDataContext FinancialMarketDataContext { get; }
        IqOptionApi.IqOptionClient IqOptionClient { get; }
        public Exception Exception { get; set; }
        TelegramService TelegramService { get; }


        public CollectCandleFactory()
        {
            FinancialMarketDataContext = new FinancialMarketDataContext();

            IqOptionClient = new IqOptionApi.IqOptionClient("rodrigo199686@hotmail.com", "rodrigoboot");
            IqOptionClient.ConnectAsync().Wait();

            TelegramService = new TelegramService();
            TelegramService.StartBot("1688028510:AAGfjRLxA3gYVWOm0oclQE6jNBjX8hUB_UY", -433344145);
        }

        public async override Task<bool> ProduceAsync()
        {
            try
            {
                const int Range = 30;
                var DiaVigente = DateTime.UtcNow.Date.AddDays(Range * -1);
                CollectCandleFactory.Running = true;

                while (DiaVigente <= DateTime.UtcNow.Date)
                {
                    TelegramService.Report($"Iniciando coleta do dia {DiaVigente:dd/MM/yyyy}");
                    FinancialMarketDataContext.Database.BeginTransaction();

                    //setup
                    var captura = new CapturaCandle(DiaVigente);

                    captura.Ativos = Ativos;
                    captura.Context = FinancialMarketDataContext;
                    captura.Timeframes = Timeframes;
                    captura.IqClient = IqOptionClient;

                    //act
                    captura.LimparAntigos(DateTime.UtcNow.Date.AddDays(-32));
                    //captura.FormatarAPartirDe(DateTime.UtcNow.AddDays(-1).Date);
                    //captura.ApagarDias();                    
                    captura.SaveCandles();
                    captura.LimparDuplicados();

                    //finish
                    DiaVigente = DiaVigente.AddDays(1);
                    FinancialMarketDataContext.Database.CommitTransaction();
                }

                CollectCandleFactory.Running = false;
                return true;
            }
            catch(Exception e)
            {
                Exception = e;
                CollectCandleFactory.Running = false;
                TelegramService.Report($"Erro na coleta dos candles: \n{e.StackTrace}");
                FinancialMarketDataContext.Database.RollbackTransaction();

                return false;
            }
        }
    }
}
