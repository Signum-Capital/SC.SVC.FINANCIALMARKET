using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SC.FINANCIALMARKET.API.Models
{
    public class ConsultaRequest
    {
        public string Nome { get; set; }
        public string Paridades { get; set; }
        public int TotalLoss { get; set; }
        public int TotalDias { get; set; }
        public int TimeFrame { get; set; }
        public int Tendencia { get; set; }
        public double PorcentagemVelas { get; set; }
        public int Gale { get; set; }
        public DateTime Data { get; set; }
        public string ConnectionId { get; set; }
    }
}
