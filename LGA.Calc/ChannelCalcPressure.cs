using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LGA.Calc
{
    public class LGACurve {
        private double[] _x;
        public double[] X
        {
            get{
                return _x;
            }
            set {
                _x = value;
            }
        }
        private double[] _y;
        public double[] Y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
            }
        }
    }
    public class ChannelCalcPressure : IChannelCalc
    {
        private bool _initialized = false;
        private DataSourceLGraph.LGraphDataChannel _data;
        private double _startPoint = 0;
        private int _period = 512;
        private double[] _fronts;
        int _numberOfPulses = 512;
        int _numberOfSegments = int.MaxValue;
        double _offset = 0;
        double _transform = 1;
        List<LGACurve> _cycles = new List<LGACurve>();
        public List<LGACurve> Cycles 
        {
            get { return _cycles; }
        }

        private LGACurve _startPointList = new LGACurve();
        public LGACurve StartPointList
        {
            get
            {
                return _startPointList;
            }
        }

        public DataSourceLGraph.LGraphDataChannel Data
        {
            get
            {
                return _data;
            }
            set
            {
                _initialized = false;
                _data = value;
            }
        }
        public void Initialize(DataSourceLGraph.LGraphDataChannel data)
        {
            _initialized = false;
            _data = data;
        }

        public void Initialize(double startPoint, int period, int numberOfPulses, double offset, double transform, int numberOfSegments, double[] fronts)
        {
            _startPoint = startPoint;
            _period = Convert.ToInt32((Convert.ToDouble(period / 360.0)) * numberOfPulses);
            _fronts = fronts;
            _initialized = true;
            _numberOfPulses = numberOfPulses;
            _offset = offset;
            _transform = transform;
            _numberOfSegments = numberOfSegments;
            if (_numberOfSegments == 0) _numberOfSegments = int.MaxValue;
        }

        
        public bool Caclculate()
        {
            if (!_initialized) return false;
            //find start point
            int startIndex = 0;
            for (; startIndex < _fronts.Length; startIndex++)
            {
                if (_fronts[startIndex] > _startPoint) break;
            }

            if (startIndex < _fronts.Length)
            {
                int numCycles = (_fronts.Length - startIndex) / _period;
                _startPointList.X = new double[Math.Min(_numberOfSegments, numCycles) + 1];
                _startPointList.Y = new double[Math.Min(_numberOfSegments, numCycles) + 1];
                _lastIndex = 0;
                _cycles = new List<LGACurve>();
                for (int indexCycle = 0; indexCycle < numCycles && indexCycle < _numberOfSegments; indexCycle++)
                {
                    List<double> xValues = new List<double>();
                    List<double> yValues = new List<double>();
                    int degree = 0;
                    int indexFront = startIndex + (indexCycle)*_period;
                    for (; indexFront < _fronts.Length && indexFront < startIndex + (indexCycle + 1) * _period; indexFront++, degree++)
                    {
                        double x,y;
                        GetValueFunc(_fronts[indexFront],_data.Times, _data.Values, out x, out y);
                        xValues.Add((double)degree / (double)_numberOfPulses * 360.0);
                        yValues.Add((y) * _transform + indexCycle * _offset);
                        if (degree == 0)
                        {
                            _startPointList.X[indexCycle] = _fronts[indexFront];
                            _startPointList.Y[indexCycle] = yValues[yValues.Count-1];
                        }
                    }
                    _startPointList.X[indexCycle+1] = _fronts[indexFront];
                    _startPointList.Y[indexCycle+1] = yValues[yValues.Count - 1];
                    _cycles.Add(new LGACurve { X = xValues.ToArray(), Y = yValues.ToArray() });
                }
                    return true;
            }
            return false;
        }

        private static int _lastIndex = 0;
        public void GetValueFunc(double t, double[] x, double[] y, out double rx, out double ry)
        {
            ry = 0;
            int i;
            for (i = _lastIndex; i < x.Length; i++)
            {
                if (t <= x[i]) break;
            }
            rx = i;
            _lastIndex = i;
            if (i < x.Length - 1)
            {
                double y1, y2;
                double t1, t2;
                y1 = y[i]; y2 = y[i + 1];
                t1 = x[i]; t2 = x[i + 1];
                double k = (y2 - y1) / (t2 - t1);
                ry = k * (t - t1) + y1;
            }
        }
    }
}


