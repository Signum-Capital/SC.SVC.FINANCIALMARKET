using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC.FINANCIALMARKET.DOMAIN.Models
{
    public class ListWithFooter
    {

        public List<ResultadoSinal> Lista { get; set; }
        public Footer Footer { get; set; }
    }
}
