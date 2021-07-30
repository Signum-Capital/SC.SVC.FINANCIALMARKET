using IqOptionApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC.FINANCIALMARKET.DOMAIN.Configuration
{
    public static class SigninIqOptionConfiguration
    {                

        private static IqOptionClient IqOptionClientStorage = new IqOptionClient("rodrigo199686@hotmail.com", "rodrigoboot");

        public static IqOptionClient IqOptionClient
        {
            get {
                if (!IqOptionClientStorage.IsConnected)
                {
                    IqOptionClientStorage.ConnectAsync().Wait();
                }

                return IqOptionClientStorage; 
            
            }
            
        }


    }
}

