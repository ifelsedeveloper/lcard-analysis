using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZedGraph;
using System.Drawing;
using LGA.DataSourceLGraph;
using LGA.Calc;

namespace LGA.ZedGraphManager
{
    static public class ZedGraphHelper
    {

        private static PointD _lastShowedPoint;
        private static PointD _lastClickedPoint;
        private static ZedGraphControl _lastZedgraphControlMenu;

        public static Action<ZedGraphControl, PointD> NewPointSelected;
        public static PointD LastClickedPoint
        {
            get { return new PointD(_lastClickedPoint.X, _lastClickedPoint.Y); }
        }

        public static void CreateGraph(ref ZedGraphControl zgc, 
            double[] x, 
            string label_x, 
            double[] y,
            string label_y,
            Color color,
            string name, 
            string title){
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


            var filteredList = new FilteredPointList(x, y);
            filteredList.SetBounds(filteredXMin, filteredXMax, filteredCount);
            LineItem myCurve = myPane.AddCurve(name, filteredList, color, SymbolType.None);
            
            // Tell ZedGraph to refigure the
            // axes since the data have changed
            //zgc.ZoomOutAll(myPane);
            
            zgc.AxisChange();
            zgc.Invalidate();
            DrawSelectedPoint(zgc);
        }

        public static void CreateGraph(ref ZedGraphControl zgc,
            List<LGACurve> curves,
            string label_x,
            string label_y,
            Color color,
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

            foreach (var curve in curves)
            {
                LineItem myCurve = myPane.AddCurve("", curve.X, curve.Y, color, SymbolType.None);
                myCurve.Tag = "0wgr";
            }
            // Tell ZedGraph to refigure the
            // axes since the data have changed
            //zgc.ZoomOutAll(myPane);
            
            zgc.AxisChange();
            zgc.Invalidate();
            DrawSelectedPoint(zgc);
        }

        public static void CreateGraph(ref ZedGraphControl zgc, LGraphData data, int maximumPointsToDisplay = 0, int numberOfFrameToDisplay = -1, int zoomDelimeter = 1, double [] xPoints = null, double[] yPoints = null)
        {
            ShowEventsOnGraph(ref zgc, 3);
            // get a reference to the GraphPane
            GraphPane myPane = zgc.GraphPane;
            // Set the Titles
            myPane.XAxis.Title.Text = "Время, " + data.TimeMarkersScale;
            myPane.YAxis.Title.Text = "В";
            myPane.Title.Text = "";
            // Очистим список кривых на тот случай, если до этого сигналы уже были нарисованы
            myPane.CurveList.Clear();

            // Make up some data arrays based on the Sine function

            // Включаем отображение сетки напротив крупных рисок по оси Y
            myPane.YAxis.MajorGrid.IsVisible = true;

            // Включаем отображение сетки напротив мелких рисок по оси X
            myPane.YAxis.MinorGrid.IsVisible = true;

            if (maximumPointsToDisplay == -1)
            {
                double filteredXMin = 0;
                double filteredXMax = data.DataChannels[0].Times[data.DataChannels[0].Times.Count() - 1];

                // Нам достаточно 20-ти точек
                int filteredCount = 15000;

                for (int i = 0; i < data.DataChannels.Count(); i++)
                {
                    if (data.DataChannels[i].Enabled)
                    {
                        var filteredList = new FilteredPointList(data.DataChannels[i].Times, data.DataChannels[i].Values);
                        filteredList.SetBounds(filteredXMin, filteredXMax, filteredCount);
                        LineItem myCurve = myPane.AddCurve("", filteredList, data.DataChannels[i].ChannelSystemColor, SymbolType.None);
                        myCurve.Tag = "wgr";
                    }
                }
            }

            if (numberOfFrameToDisplay != -1)
            {
                for (int i = 0; i < data.DataChannels.Count(); i++)
                {
                    if (data.DataChannels[i].Enabled)
                    {
                        int k = 0;
                        List<double> x = new List<double>();
                        List<double> y = new List<double>();
                        int startPoint = numberOfFrameToDisplay*(maximumPointsToDisplay/zoomDelimeter);
                        //create subarray
                        if ((numberOfFrameToDisplay*(maximumPointsToDisplay/zoomDelimeter) + maximumPointsToDisplay) > data.KadrsNumber) 
                            startPoint = data.KadrsNumber - maximumPointsToDisplay - 1;
                        for (int j = startPoint; j < data.KadrsNumber && k < maximumPointsToDisplay; j++)
                        {
                            x.Add(data.DataChannels[i].Times[j]);
                            y.Add(data.DataChannels[i].Values[j]);
                            k++;
                        }
                        var xValues = x.ToArray();
                        var yValues = y.ToArray();
                        myPane.XAxis.Scale.Min = xValues.Min();
                        myPane.XAxis.Scale.Max = xValues.Max();
                        LineItem myCurve = myPane.AddCurve("", xValues, yValues, data.DataChannels[i].ChannelSystemColor, SymbolType.None);
                        myCurve.Tag = "wgr";
                    }

                }
                if (xPoints != null && yPoints != null)
                {
                    List<double> xDrawList = new List<double>();
                    List<double> yDrawList = new List<double>();
                    for(int i=0; i < xPoints.Count(); i++)
                    {
                        if (xPoints[i] > myPane.XAxis.Scale.Max) break;
                        if (xPoints[i] > myPane.XAxis.Scale.Min)
                        {
                            xDrawList.Add(xPoints[i]);
                            yDrawList.Add(yPoints[i]);
                        }
                    }

                    LineItem myCurveInf = myPane.AddCurve("", xDrawList.ToArray(), yDrawList.ToArray(), System.Drawing.Color.DarkRed, SymbolType.Triangle);

                    myCurveInf.Symbol.Fill.Color = System.Drawing.Color.SeaGreen;
                    myCurveInf.Line.IsVisible = false;
                    myCurveInf.Line.Width = 3;
                    // !!!
                    // Тип заполнения - сплошная заливка
                    myCurveInf.Symbol.Fill.Type = FillType.Solid;
                    // !!!
                    // Размер ромбиков
                    myCurveInf.Symbol.Size = 6;
                }
            }

            // Tell ZedGraph to refigure the
            // axes since the data have changed
            //zgc.ZoomOutAll(myPane);
            zgc.AxisChange();
            zgc.Invalidate();
            DrawSelectedPoint(zgc);
        }
        /// <summary>
        /// Обработчик события, который вызывается, перед показом контекстного меню
        /// </summary>
        /// <param name="sender">Компонент ZedGraph</param>
        /// <param name="menuStrip">Контекстное меню, которое будет показано</param>
        /// <param name="mousePt">Координаты курсора мыши</param>
        /// <param name="objState">Состояние контекстного меню. Описывает объект, на который кликнули.</param>
        public static void zedGraph_ContextMenuBuilder(ZedGraphControl sender,
            System.Windows.Forms.ContextMenuStrip menuStrip,
            Point mousePt,
            ZedGraphControl.ContextMenuObjectState objState)
        {

            // Добавим свой пункт меню
            System.Windows.Forms.ToolStripItem newMenuItem = new System.Windows.Forms.ToolStripMenuItem("Выбрать как начальную точку");
            newMenuItem.Click += selectPoint_Click;
            menuStrip.Items.Add(newMenuItem);
            _lastZedgraphControlMenu = sender;
            double x, y;
            // Пересчитываем пиксели в координаты на графике
            // У ZedGraph есть несколько перегруженных методов ReverseTransform.
            sender.GraphPane.ReverseTransform(mousePt, out x, out y);
            x = Math.Round(x, 6);
            _lastShowedPoint = new PointD(x, y);
        }

        static void selectPoint_Click(object sender, EventArgs e)
        {
            _lastClickedPoint = _lastShowedPoint;
            DrawSelectedPoint(_lastZedgraphControlMenu);
            NewPointSelected(_lastZedgraphControlMenu,_lastClickedPoint);
        }

        private static void DrawSelectedPoint(ZedGraphControl zgc)
        {
            GraphPane myPane = zgc.GraphPane;
                // !!! Минимум
                // Создадим кривую с названием "Scatter".
                // Обводка ромбиков будут рисоваться голубым цветом (Color.Blue),
                // Опорные точки - ромбики (SymbolType.Diamond)
                myPane.CurveList.RemoveAll(curve => (string)curve.Tag == "selectedPoint");
                LineItem myCurveInf = myPane.AddCurve("", new[] { _lastClickedPoint.X, _lastClickedPoint.X }, new[] { myPane.YAxis.Scale.Min, myPane.YAxis.Scale.Max }, System.Drawing.Color.Black, SymbolType.None);
                myPane.CurveList.Move(0, 999);
                myCurveInf.Tag = "selectedPoint";
                // !!!
                // У кривой линия будет невидимой
                myCurveInf.Line.IsVisible = true;

                // !!!
                // Цвет заполнения отметок (ромбиков) - колубой
                //myCurveInf.Symbol.Fill.Color = System.Drawing.Color.Red;
                myCurveInf.Line.Width = 2;
                // !!!
                // Тип заполнения - сплошная заливка
                //myCurveInf.Symbol.Fill.Type = FillType.Solid;
                // !!!
                // Размер ромбиков
                myCurveInf.Symbol.Size = 10;

           //zgc.AxisChange();
           zgc.Invalidate();
               
        }

        public static void AddNewPointToGraph(ZedGraphControl zgc, double x, double y)
        {
            GraphPane myPane = zgc.GraphPane;
            var curve = myPane.CurveList.FirstOrDefault(c => c.Points.Count > 25);
            if (curve != null)
            {
                for (int i = 0; i < curve.Points.Count-1; i++)
                {
                    if (curve.Points[i].X >= x)
                    {
                        y = (curve.Points[i].Y + curve.Points[i + 1].Y)/2;
                        break;
                    }
                }
            }
            LineItem myCurveInf = myPane.AddCurve("", new[] { x }, new[] { y }, System.Drawing.Color.Red, SymbolType.Star);
            myPane.CurveList.Move(0, 999);
            myCurveInf.Tag = "selectedPoint";
            // !!!
            // У кривой линия будет невидимой
            myCurveInf.Line.IsVisible = false;

            // !!!
            // Цвет заполнения отметок (ромбиков) - колубой
            myCurveInf.Symbol.Fill.Color = System.Drawing.Color.Red;
            myCurveInf.Line.Width = 2;
            // !!!
            // Тип заполнения - сплошная заливка
            myCurveInf.Symbol.Fill.Type = FillType.Solid;
            // !!!
            // Размер ромбиков
            myCurveInf.Symbol.Size = 10;

            //zgc.AxisChange();
            zgc.Invalidate();

        }

        public static void AddSelectPointAction(ZedGraphControl zgc)
        {
            zgc.ContextMenuBuilder +=
            zedGraph_ContextMenuBuilder;
        }

        public static void ShowEventsOnGraph(ref ZedGraphControl zgc, int digits = 0)
        {
            if (!zgc.IsShowPointValues)
            {
                // Включим показ всплывающих подсказок при наведении курсора на график
                zgc.IsShowPointValues = true;

                // Будем обрабатывать событие PointValueEvent, чтобы изменить формат представления координат
                if (digits == 0)
                    zgc.PointValueEvent += zedGraph_PointValueEvent;
                else
                    zgc.PointValueEvent += zedGraph_PointValue3DigitsEvent;
            }}

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
            string result = string.Format("X: {0:F3}\nY: {1:F0}", point.X, point.Y);
            //_lastShowedPoint = new PointD(curve[iPt].X, curve[iPt].Y);
            return result;
        }

        public static string zedGraph_PointValue3DigitsEvent(ZedGraphControl sender,
            GraphPane pane,
            CurveItem curve,
            int iPt)
        {
            // Получим точку, около которой находимся
            PointPair point = curve[iPt];

            // Сформируем строку
            string result = string.Format("X: {0:F3}\nY: {1:F3}", point.X, point.Y);
            //_lastShowedPoint = new PointD(curve[iPt].X, curve[iPt].Y);
            return result;
        }

    }
}


