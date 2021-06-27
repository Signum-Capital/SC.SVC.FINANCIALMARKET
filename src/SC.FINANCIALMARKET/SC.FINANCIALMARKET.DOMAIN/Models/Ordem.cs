using IqOptionApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC.FINANCIALMARKET.DOMAIN.Models
{
    public class Ordem
    {
        public Ordem()
        {
        }

        public Ordem(double porcentagem, OrderDirection? ordemCandle)
        {
            Porcentagem = Convert.ToInt32(porcentagem);
            OrdemCandle = ordemCandle;
        }

        public int Porcentagem { get; set; }
        public OrderDirection? OrdemCandle { get; set; }
    }
}
