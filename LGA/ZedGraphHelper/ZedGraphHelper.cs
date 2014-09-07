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

        private static PointD _lastShowedPoint = new PointD();
        private static PointD _lastClickedPoint = new PointD();
        private static ZedGraphControl _lastZedgraphControlMenu = null;

        public static Action<ZedGraphControl, PointD> newPointSelected;
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


            FilteredPointList filteredList = new FilteredPointList(x, y);
            filteredList.SetBounds(filteredXMin, filteredXMax, filteredCount);
            LineItem myCurve = myPane.AddCurve(name, filteredList, color, SymbolType.None);
            
            // Tell ZedGraph to refigure the
            // axes since the data have changed
            //zgc.ZoomOutAll(myPane);
            zgc.AxisChange();
            zgc.Invalidate();
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
        }

        static void selectPoint_Click(object sender, EventArgs e)
        {
            _lastClickedPoint = _lastShowedPoint;
            DrawSelectedPoint(_lastZedgraphControlMenu);
            newPointSelected(_lastZedgraphControlMenu,_lastClickedPoint);
        }

        private static void DrawSelectedPoint(ZedGraphControl zgc)
        {
            GraphPane myPane = zgc.GraphPane;
                // !!! Минимум
                // Создадим кривую с названием "Scatter".
                // Обводка ромбиков будут рисоваться голубым цветом (Color.Blue),
                // Опорные точки - ромбики (SymbolType.Diamond)
                myPane.CurveList.RemoveAll(curve => (string)curve.Tag == "selectedPoint");
                LineItem myCurveInf = myPane.AddCurve("", new double[] { _lastClickedPoint.X }, new double[] { _lastClickedPoint.Y }, System.Drawing.Color.Red, SymbolType.Diamond);
                myCurveInf.Tag = "selectedPoint";
                // !!!
                // У кривой линия будет невидимой
                myCurveInf.Line.IsVisible = false;

                // !!!
                // Цвет заполнения отметок (ромбиков) - колубой
                myCurveInf.Symbol.Fill.Color = System.Drawing.Color.Red;
                myCurveInf.Line.Width = 4;
                // !!!
                // Тип заполнения - сплошная заливка
                myCurveInf.Symbol.Fill.Type = FillType.Solid;
                // !!!
                // Размер ромбиков
                myCurveInf.Symbol.Size = 10;

           zgc.AxisChange();
           zgc.Invalidate();
               
        }

        public static void AddSelectPointAction(ZedGraphControl zgc)
        {
            zgc.ContextMenuBuilder +=
            new ZedGraphControl.ContextMenuBuilderEventHandler(ZedGraphManager.ZedGraphHelper.zedGraph_ContextMenuBuilder);
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
            _lastShowedPoint = new PointD(curve[iPt].X, curve[iPt].Y);
            return result;
        }

        /// <summary>
        /// Обработчик события перемещения точки.
        /// При перемещении точки, информация о ней записывается в заголовок окна
        /// </summary>
        /// <param name="sender">Компонент ZedGraph</param>
        /// <param name="pane">Панель с графиком</param>
        /// <param name="curve">Кривая, точку которой переместили</param>
        /// <param name="iPt">Номер точки</param>
        /// <returns>Метод должен возвращать строку</returns>
        static string zedGraph_PointEditEvent(ZedGraphControl sender,
            GraphPane pane, CurveItem curve, int iPt)
        {
            string title = string.Format("Начальная точка координаты: ({1}; {2})", curve[iPt].X, curve[iPt].Y);

            _lastClickedPoint = new PointD(curve[iPt].X, curve[iPt].Y);

            return title;
        }

    }
}
