using System.Text.Json.Serialization;

namespace SC.FINANCIALMARKET.DOMAIN.Models
{
    class Item
    {
        public Item(Candle candle, int timezone)
        {
            Candle = candle;
            Timezone = timezone;
        }

        [JsonIgnore]
        public int Timezone { get; set; }

        [JsonIgnore]
        public Candle Candle { get; set; }

        [JsonIgnore]
        public OrderDirection? Order
        {
            get
            {
                return Candle.Abertura > Candle.Fechamento ? OrderDirection.Put : Candle.Abertura == Candle.Fechamento ? null : OrderDirection.Call;
            }
        }

        public string Data
        {
            get
            {
                return Candle.getWithTimeZone(Timezone);
            }
        }

        public string Cor
        {
            get
            {
                return Order switch
                {
                    OrderDirection.Call => "#82FF97",
                    OrderDirection.Put => "#FF4D4D",
                    _ => "#0D8CFF"
                };
            }
        }

        public string Abertura
        {
            get
            {
                return Candle.Abertura.ToString();
            }
        }

        public string Fechamento
        {
            get
            {
                return Candle.Fechamento.ToString();
            }
        }
    }
}
