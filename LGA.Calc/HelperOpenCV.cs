using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace LGA.Calc
{
    public class HelperOpenCV
    {
        public static double[] filterSmooth(double[] x, double[] y, int numberPoints, SmoothType typeFilter)
        {
            double[] res = new double[y.LongLength];
            CvMat Mat = new CvMat(1, y.Length, MatrixType.F64C1);
            Random rnd = new Random();
            for (int i = 0; i < y.Length; i++)
                Mat.DataArrayDouble[i] = Math.Abs(y[i]);

            Cv.Smooth(Mat, Mat, typeFilter, numberPoints);

            for (int i = 0; i < y.Length; i++)
                res[i] = Mat.DataArrayDouble[i];

            return res;
        }

        public static double[] SmoothGauss(double[] x, double[] y, int numberPoints)
        {
            double[] res = new double[y.LongLength];
            CvMat Mat = new CvMat(1, y.Length, MatrixType.F64C1);
            CvMat MatRes = new CvMat(1, y.Length, MatrixType.F64C1);
            for (int i = 0; i < y.Length; i++)
                Mat.DataArrayDouble[i] = Math.Abs(y[i]);
            Cv.Smooth(Mat, MatRes, SmoothType.Gaussian, numberPoints, numberPoints);
            for (int i = 0; i < y.Length; i++)
                res[i] = MatRes.DataArrayDouble[i];
            return res;
        }
    }
}
