using SC.FINANCIALMARKET.DOMAIN.Models;
using SC.PKG.SERVICES.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SC.FINANCIALMARKET.DOMAIN.Factories
{
    public class PatternizedListFactory : Factory<List<ResultadoSinal>>
    {
        public EntradaSinal Entrada { get; set; }

        public PatternizedListFactory(EntradaSinal entrada)
        {
            Entrada = entrada;
        }

        public override async Task<List<ResultadoSinal>> ProduceAsync()
        {

            var ListaBruta = Entrada.Lista.Split('\n');
            var horario = 0;
            List<ResultadoSinal> ListaTratada = new List<ResultadoSinal>();

            foreach (var linha in ListaBruta)
            {
                var colunas = linha.Split(',', ';', ' ');

                var res = new ResultadoSinal();
                

                for (int i = 0; i < colunas.Length; i++)
                {
                    var parametro = colunas[i].Trim('\"');
                    
                    if (RegexHora.IsMatch(parametro))
                    {
                        var hora = parametro.Split(':');
                        res.Horario = Entrada.Data.Date.AddHours(Int32.Parse(hora[0])).AddMinutes(Int32.Parse(hora[1]));
                    }
                    else if (RegexTimeframe.IsMatch(parametro))
                    {
                        var time = parametro.Replace("M", "").Replace("m", "");
                        res.Timeframe = Int32.Parse(time);
                        horario = res.Timeframe = Int32.Parse(time); 
                    }
                    else if (RegexOrdem.IsMatch(parametro))
                    {
                        res.Ordem = parametro;
                    }
                    else if (RegexParidade.IsMatch(parametro))
                    {
                        res.Paridade = parametro;
                    }
                    else if (RegexGale.IsMatch(parametro))
                    {
                        var gale = parametro.Replace("G", "").Replace("g", "");
                        res.Gale = Int32.Parse(gale);
                    }

                    if (res.Timeframe == 0)
                    {
                        res.Timeframe = horario;
                    }

                }

                if (!(res.Paridade == null || res.Timeframe == 0 || res.Ordem == null))
                {
                    ListaTratada.Add(res);
                }
                
            }

            return ListaTratada;
        }

        private Regex RegexHora
        {
            get => new Regex("\\d{2}:\\d{2}");
        }

        private Regex RegexTimeframe
        {
            get => new Regex("(\\d{1,}(m|M)|(m|M)\\d{1,})");
        }

        private Regex RegexOrdem
        {
            get => new Regex("(put|call)", RegexOptions.IgnoreCase);
        }

        private Regex RegexParidade
        {
            get => new Regex("(\\w{6}|\\w{3}\\/\\w{3})(_OTC|-OTC|\\/OTC|)", RegexOptions.IgnoreCase);
        }

        private Regex RegexGale 
        {
            get => new Regex("\\d{1}G", RegexOptions.IgnoreCase);
        }


}
}
