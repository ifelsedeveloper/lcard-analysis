using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LGA.Calc
{
    public class RecordCalculation
    {
        private DataSourceLGraph.LGraphData _data = null;
        private IList<IChannelCalc> _calcChannels;
        public RecordCalculation(DataSourceLGraph.LGraphData data)
        {
            _data = data;
            _calcChannels = FactoryChannelCalc.createChannelsCalc(data);
        }

        public ChannelCalcFrequency getFrequencyCalc()
        {
            return (ChannelCalcFrequency)_calcChannels.Where(calc => calc.GetType() == typeof(ChannelCalcFrequency)).FirstOrDefault();
        }

        public ChannelCalcPressure getPressureCalc()
        {
            return (ChannelCalcPressure)_calcChannels.Where(calc => calc.GetType() == typeof(ChannelCalcPressure)).FirstOrDefault();
        }
    }
}
