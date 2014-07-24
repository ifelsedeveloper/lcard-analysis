using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LGA.DataSourceLGraph
{
    public class LGraphDataChannel : INotifyPropertyChangedHelper
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private Color _channelColor;
        public Color ChannelColor
        {
            get { return _channelColor; }
            set
            {
                _channelColor = value;
                OnPropertyChanged();
            }
        }

        private bool _enabled;
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                OnPropertyChanged();
            }
        }

        public double[] Values;
        public double[] Times;
        
    }
}
