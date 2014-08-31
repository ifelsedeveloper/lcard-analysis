using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LGA.DataSourceLGraph
{
    public enum CannelType
    {
        Pressure = 0,
        Frequency = 1,
        Fuel = 2
    }

    public class LGraphDataType : INotifyPropertyChangedHelper, IComparable<LGraphDataType>
    {
        CannelType _id;
        public CannelType Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }


        public int CompareTo(LGraphDataType dataType)
        {
            if (dataType == null) return 1;
            return _id.CompareTo(dataType.Id);
        }
    }
}
