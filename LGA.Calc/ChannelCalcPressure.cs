using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LGA.Calc
{
    public class ChannelCalcPressure : IChannelCalc
    {
        private DataSourceLGraph.LGraphDataChannel _data;
        public DataSourceLGraph.LGraphDataChannel Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
            }
        }
        public void Initialize(DataSourceLGraph.LGraphDataChannel data)
        {

        }

        public bool Caclculate()
        {
            return false;
        }
    }
}
