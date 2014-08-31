using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LGA.Calc
{
    public class FactoryChannelCalc
    {
        public static IList<IChannelCalc> createChannelsCalc(DataSourceLGraph.LGraphData data)
        {
            IList<IChannelCalc> res = new List<IChannelCalc>();
            foreach (var channel in data.DataChannels)
            {
                switch (channel.TypeChannel)
                {
                    case DataSourceLGraph.CannelType.Frequency:
                        res.Add(new ChannelCalcFrequency() { Data = channel });
                        break;
                    case DataSourceLGraph.CannelType.Pressure:
                        res.Add(new ChannelCalcPressure() { Data = channel });
                        break;
                    case DataSourceLGraph.CannelType.Fuel:
                        res.Add(new ChannelCalcPressure() { Data = channel });
                        break;
                }
            }
            return res;
        }
    }
}
