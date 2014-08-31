using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LGA.Calc
{
    public class ChannelCalcFrequency : IChannelCalc
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

        private int number_zub = 360;
        double[] fronts;
        double[] data_vu;

        double[] t_vu;
        public double[] T_vu
        {
            get { return t_vu; }
        }

        double[] degree_vu;

        double[] vu;
        public double[] Vu
        {
            get { return vu; }
        }

        public void Initialize(DataSourceLGraph.LGraphDataChannel data)
        {
            _data = data;
        }

        public bool Caclculate()
        {
            CalcFrequency();
            return true;
        }

        public double[] CalcFront()
        {
            double[] res = null;
            int q = 0;
            int i;
            double median_value = _data.Values.Average();
            int kol_front = 0;
            //подсчёт количества фронтов
            for (i = 0; i < _data.Values.Length - 4; i++)
            {

                if (_data.Values[i] > median_value && 
                    _data.Values[i + 1] > median_value && 
                    _data.Values[i + 2] > median_value)
                {
                    q++; i++;
                    for (; (i < _data.Values.Length - 4) && 
                            _data.Values[i] > median_value && 
                            _data.Values[i + 1] > median_value && 
                            _data.Values[i + 2] > median_value; 
                            i++) ;
                }
            }
            kol_front = q;

            q = 0;
            res = new double[kol_front];
            for (i = 0; i < _data.Values.Length - 4; i++)
            {
                if (_data.Values[i] > median_value && _data.Values[i+1] > median_value && _data.Values[i+2] > median_value)
                {
                    res[q] = _data.Times[i]; q++; i++;
                    for (; (i < _data.Values.Length - 4) && 
                            _data.Values[i] > median_value && 
                            _data.Values[i + 1] > median_value && 
                            _data.Values[i + 2] > median_value; 
                        i++)
                    { }
                }
            }

            return res;
        }

        void CalcFrequency()
        {
            fronts = CalcFront();
            data_vu = new double[fronts.Length-1];
            t_vu = new double[fronts.Length-1];
            degree_vu = new double[fronts.Length-1];

            int i;
            for (i = 0; i < fronts.Length-1; i++)
            {
                data_vu[i] = 60.0 / ((fronts[i + 1] - fronts[i]) * Convert.ToDouble(number_zub));
                t_vu[i] = (fronts[i + 1] + fronts[i]) / 2.0;
                degree_vu[i] = i + 0.5;
            }

            vu = HelperOpenCV.SmoothGauss(t_vu, data_vu, 3);

        }
    }
}
