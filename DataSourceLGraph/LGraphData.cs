using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LGA.DataSourceLGraph
{
    public class LGraphData : INotifyPropertyChangedHelper
    {
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        private string _experimentTime;
        public string ExperimentTime
        {
            get { return _experimentTime; }
            set
            {
                _experimentTime = value;
                OnPropertyChanged();
            }
        }

        private string _module;
        public string Module
        {
            get { return _module; }
            set
            {
                _module = value;
                OnPropertyChanged();
            }
        }

        private int _numberOfChannels;
        public int NumberOfChannels
        {
            get { return _numberOfChannels; }
            set
            {
                _numberOfChannels = value;
                OnPropertyChanged();
            }
        }

        private int _kadrsNumber;
        public int KadrsNumber
        {
            get { return _kadrsNumber; }
            set
            {
                _kadrsNumber = value;
                OnPropertyChanged();
            }
        }

        private double? _inputRateInkHz;
        public double? InputRateInkHz
        {
            get { return _inputRateInkHz; }
            set
            {
                _inputRateInkHz = value;
                OnPropertyChanged();
            }
        }

        private double? _inputTimeInSec;
        public double? InputTimeInSec
        {
            get { return _inputTimeInSec; }
            set
            {
                _inputTimeInSec = value;
                OnPropertyChanged();
            }
        }

        private int _decimation;
        public int Decimation
        {
            get { return _decimation; }
            set
            {
                _decimation = value;
                OnPropertyChanged();
            }
        }

        private string _timeMarkersScale;
        public string TimeMarkersScale
        {
            get { return _timeMarkersScale; }
            set
            {
                _timeMarkersScale = value;
                OnPropertyChanged();
            }
        }

        public double[] time;//время 
        public double[][] ch;//записанные каналы

        public ObservableCollection<LGraphHeaderItem> HeaderItems = new ObservableCollection<LGraphHeaderItem>();
        public ObservableCollection<LGraphDataChannel> DataChannels = new ObservableCollection<LGraphDataChannel>();
    }
}
