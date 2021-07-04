using IqOptionApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC.FINANCIALMARKET.DOMAIN.Collections
{
    public class ValidActivesPairs
    {
        public static List<ActivePair> GetActivePairs
        { 
            get => new List<ActivePair>()
            {
                ActivePair.AUDCAD,
                ActivePair.AUDCAD_OTC,
                ActivePair.AUDCHF,
                ActivePair.AUDJPY,
                ActivePair.AUDUSD,
                ActivePair.CADJPY,
                ActivePair.EURAUD,
                ActivePair.EURCAD,
                ActivePair.EURCHF,
                ActivePair.EURGBP,
                ActivePair.EURGBP_OTC,
                ActivePair.EURJPY,
                ActivePair.EURJPY_OTC,
                ActivePair.EURNZD,
                ActivePair.EURUSD,
                ActivePair.EURUSD_OTC,
                ActivePair.GBPAUD,
                ActivePair.GBPCAD,
                ActivePair.GBPCHF,
                ActivePair.GBPJPY,
                ActivePair.GBPJPY_OTC,
                ActivePair.GBPNZD,
                ActivePair.GBPUSD,
                ActivePair.GBPUSD_OTC,
                ActivePair.NZDCHF,
                ActivePair.NZDUSD,
                ActivePair.NZDUSD_OTC,
                ActivePair.USDCAD,
                ActivePair.USDCHF,
                ActivePair.USDCHF_OTC,
                ActivePair.USDJPY,
                ActivePair.USDJPY_OTC,
            };
        }
    }
}
