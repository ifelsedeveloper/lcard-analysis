using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LGA.Calc
{
    public interface IChannelCalc
    {
        bool Caclculate();

        void Initialize(DataSourceLGraph.LGraphDataChannel data);
    }
}
