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

        private int number_zub = 512;
        private int number_smooth = 65;
        private int number_segments = 1000;

        double[] fronts;
        public double[] Fronts
        {
            get { return fronts; }
        }

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

        public int with_diff = 5;
        private double[] vu_ac;
        public double[] Vu_ac
        {
            get { return vu_ac; }
        }
        private double[] ac;
        public double[] Ac
        {
            get { return ac; }
        }
        private double[] t_ac;
        public double[] T_ac
        {
            get { return t_ac; }
        }

        public double[] degree_ac;

        public void Initialize(DataSourceLGraph.LGraphDataChannel data)
        {
            _data = data;
        }

        public void Initialize(int NumberOfPulses, int NumberOfSmooth, int SegmentsNumber)
        {
            number_zub = NumberOfPulses;
            number_smooth = NumberOfSmooth;
            number_segments = SegmentsNumber;
        }
        public bool Caclculate()
        {
            CalcFrequency();
            CalcAcceleration();
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
                    if (q > number_segments * number_zub) break;
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
                        i++) ;

                }
                if (q > number_segments * number_zub) break;
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
            if (number_smooth % 2 == 0) number_smooth += 1;
            vu = HelperOpenCV.SmoothGauss(t_vu, data_vu, number_smooth);

        }
        void CalcAcceleration()
        {
            int kol_ac = t_vu.Length - 2 - 2 * with_diff;
            vu_ac = new double[kol_ac];
            ac = new double[kol_ac];
            t_ac = new double[kol_ac];
            degree_ac = new double[kol_ac];
            double k;   //пароизводная частоты вращения по времени
            double x;   //параметр прямой
            int i;
            double v1;
            double v2;
            double t1;
            double t2;
            int j;

            //расчёт ускорения
            for (i = 0; i < kol_ac; i++)
            {
                v1 = 0;
                v2 = 0;
                t1 = 0;
                t2 = 0;
                for (j = i; j < i + with_diff; j++)
                {
                    v1 += vu[j];
                    v2 += vu[j + with_diff];
                    t1 += t_vu[j];
                    t2 += t_vu[j + with_diff];

                }
                k = ((v2 - v1) / 60.0) / (t2 - t1);
                x = (t1 + t2) / (2.0 * with_diff);
                vu_ac[i] = (v2 + v1) / (2.0 * with_diff);
                ac[i] = k * 2 * Math.PI / 60.0;
                t_ac[i] = x;
                degree_ac[i] = (degree_vu[i] + degree_vu[i + with_diff - 1]) / 2.0;
            }
        }
    }
}
