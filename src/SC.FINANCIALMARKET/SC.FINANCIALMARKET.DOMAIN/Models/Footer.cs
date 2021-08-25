using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC.FINANCIALMARKET.DOMAIN.Models
{
    public class Footer
    {
        public Footer(int total, int apurado, int winPrimeira, int winGale, int percentualWinPrimeira, int percentualWinGale, int assertividade)
        {
            Total = total;
            Apurado = apurado;
            WinPrimeira = winPrimeira;
            WinGale = winGale;
            PercentualWinPrimeira = percentualWinPrimeira;
            PercentualWinGale = percentualWinGale;
            Assertividade = assertividade;
        }

        public int Total { get; set; }
        public int Apurado { get; set; }
        public int WinPrimeira { get; set; }
        public int WinGale { get; set; }
        public int PercentualWinPrimeira { get; set; }
        public int PercentualWinGale { get; set; }
        public int Assertividade { get; set; }

    }
}
