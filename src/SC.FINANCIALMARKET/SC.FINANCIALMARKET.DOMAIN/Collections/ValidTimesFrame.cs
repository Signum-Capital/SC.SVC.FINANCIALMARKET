using IqOptionApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC.FINANCIALMARKET.DOMAIN.Collections
{
    public class ValidTimesFrame
    {
        public static List<TimeFrame> TimeFrames
        {
            get => new List<TimeFrame>()
            {
                TimeFrame.Min1,
                TimeFrame.Min5,
                TimeFrame.Min15,
                TimeFrame.Min30
            };
        }
    }
}
