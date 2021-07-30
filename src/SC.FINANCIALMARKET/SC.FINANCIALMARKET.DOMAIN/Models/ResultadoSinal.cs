﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC.FINANCIALMARKET.DOMAIN.Models
{
    public class ResultadoSinal
    {
        public string Paridade { get; set; }
        public string Ordem { get; set; }
        public DateTime Horario { get; set; }
        public string Resultado { get; set; }
        public int Timeframe { get; set; }
        public int Gale { get; set; }

    }
}
