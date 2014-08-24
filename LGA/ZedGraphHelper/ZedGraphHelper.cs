using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZedGraph;
using System.Drawing;
using LGA.DataSourceLGraph;

namespace LGA.ZedGraphHelper
{
    static public class ZedGraphHelper
    {
        public static void CreateGraph(ref ZedGraphControl zgc, 
            ref double[] x, 
            string label_x, 
            ref double[][] y,
            Color[] colors,
            string label_y,
            string name, 
            string title)
        {
            ShowEventsOnGraph(ref zgc);
            // get a reference to the GraphPane
            GraphPane myPane = zgc.GraphPane;
            // Set the Titles
            myPane.Title.Text = title;
            myPane.XAxis.Title.Text = label_x;
            myPane.YAxis.Title.Text = label_y;

            // Очистим список кривых на тот случай, если до этого сигналы уже были нарисованы
            myPane.CurveList.Clear();

            // Make up some data arrays based on the Sine function

            // Включаем отображение сетки напротив крупных рисок по оси Y
            myPane.YAxis.MajorGrid.IsVisible = true;

            // Включаем отображение сетки напротив мелких рисок по оси X
            myPane.YAxis.MinorGrid.IsVisible = true;

            double filteredXMin = 0;
            double filteredXMax = x[x.Count() - 1];

            // Нам достаточно 20-ти точек
            int filteredCount = 10000;

            for (int i = 0; i < y.Count(); i++)
            {
                FilteredPointList filteredList = new FilteredPointList(x, y[i]);
                filteredList.SetBounds(filteredXMin, filteredXMax, filteredCount);
                LineItem myCurve = myPane.AddCurve(name, filteredList, colors[i], SymbolType.None);
            }
            
            // Tell ZedGraph to refigure the
            // axes since the data have changed
            //zgc.ZoomOutAll(myPane);
            zgc.AxisChange();
            zgc.Invalidate();
        }

        public static void CreateGraph(ref ZedGraphControl zgc, IList<LGraphDataChannel> channels)
        {
            ShowEventsOnGraph(ref zgc);
            // get a reference to the GraphPane
            GraphPane myPane = zgc.GraphPane;
            // Set the Titles
            myPane.XAxis.Title.Text = "Время, с";
            myPane.YAxis.Title.Text = "";
            myPane.Title.Text = "";
            // Очистим список кривых на тот случай, если до этого сигналы уже были нарисованы
            myPane.CurveList.Clear();

            // Make up some data arrays based on the Sine function

            // Включаем отображение сетки напротив крупных рисок по оси Y
            myPane.YAxis.MajorGrid.IsVisible = true;

            // Включаем отображение сетки напротив мелких рисок по оси X
            myPane.YAxis.MinorGrid.IsVisible = true;

            double filteredXMin = 0;
            double filteredXMax = channels[0].Times[channels[0].Times.Count() - 1];

            // Нам достаточно 20-ти точек
            int filteredCount = 10000;

            for (int i = 0; i < channels.Count(); i++)
            {
                if (channels[i].Enabled)
                {
                    FilteredPointList filteredList = new FilteredPointList(channels[i].Times, channels[i].Values);
                    filteredList.SetBounds(filteredXMin, filteredXMax, filteredCount);
                    LineItem myCurve = myPane.AddCurve("", filteredList, channels[i].ChannelSystemColor, SymbolType.None);
                }
            }

            // Tell ZedGraph to refigure the
            // axes since the data have changed
            //zgc.ZoomOutAll(myPane);
            zgc.AxisChange();
            zgc.Invalidate();
        }

        public static void ShowEventsOnGraph(ref ZedGraphControl zgc)
        {
            if (!zgc.IsShowPointValues)
            {
                // Включим показ всплывающих подсказок при наведении курсора на график
                zgc.IsShowPointValues = true;

                // Будем обрабатывать событие PointValueEvent, чтобы изменить формат представления координат
                zgc.PointValueEvent +=
                    new ZedGraphControl.PointValueHandler(zedGraph_PointValueEvent);
            }
        }

        /// <summary>
        /// Обработчик события PointValueEvent.
        /// Должен вернуть строку, которая будет показана во всплывающей подсказке
        /// </summary>
        /// <param name="sender">Отправитель сообщения</param>
        /// <param name="pane">Панель для рисования</param>
        /// <param name="curve">Кривая, около которой находится курсор</param>
        /// <param name="iPt">Номер точки в кривой</param>
        /// <returns>Нужно вернуть отображаемую строку</returns>
        public static string zedGraph_PointValueEvent(ZedGraphControl sender,
            GraphPane pane,
            CurveItem curve,
            int iPt)
        {
            // Получим точку, около которой находимся
            PointPair point = curve[iPt];

            // Сформируем строку
            string result = string.Format("X: {0:F3}\nY: {1:F3}", point.X, point.Y);

            return result;
        }

    }
}
