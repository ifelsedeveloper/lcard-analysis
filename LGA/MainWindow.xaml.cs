using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LGA.DataSourceLGraph;
using DevExpress.Xpf.Charts;
using System.Data;
using System.Collections.ObjectModel;
using DevExpress.Xpf.Core;
using System.Windows.Forms.Integration;
using DevExpress.XtraCharts;
using ZedGraph;
using LGA.Calc;
using LGA.Dialogs;

namespace LGA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        LGraphData currentRecord;
        ZedGraphControl graphControl = new ZedGraphControl();
        private static string tagTabFrequency = "tabFreq";
        private static string tagAccTime = "tabAccTime";
        private static string tagPressure = "tabPressure";
        private static PointD _selectedPoint = new PointD();

        public MainWindow()
        {
            InitializeComponent();
            ZedGraphManager.ZedGraphHelper.NewPointSelected += StartPointSelected;
            ZedGraphManager.ZedGraphHelper.AddSelectPointAction(graphControl);
            graphHost.Child = graphControl;
        }

        private void StartPointSelected(ZedGraphControl zgc, PointD value)
        {
            _selectedPoint = value;
            Properties.Settings.Default.StartPoint = value.X;
            Properties.Settings.Default.Save();
        }

        private void ExitCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        
        private void OpenFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text documents (.txt)|*.txt";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                string filename = dlg.FileName;
                DataManager dataManager = new DataManager();
                if (dataManager.readHeader(filename) != null)
                {
                    // Open document
                    currentAction.Text = "Чтение данных...";
                    dataManager.ReadDataAsync(filename, data =>
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            newFileOpened(data);
                        }));

                    });
                }
                else 
                {
                    MessageBox.Show("Файл не содержит данных или имеет некорректный формат.");
                }
            }
        }

        private void newFileOpened(LGraphData data)
        {
            currentRecord = data;
            currentAction.Text = "Чтение данных завершено";
            dataGridFileProperty.ItemsSource = data.HeaderItems;
            dataGridChannelProperty.ItemsSource = data.DataChannels;
            ZedGraphManager.ZedGraphHelper.CreateGraph(ref graphControl, currentRecord);
            Properties.Settings.Default.StartPoint = 0;
            Properties.Settings.Default.Save();
            foreach (var dataChannel in data.DataChannels)
            {
                dataChannel.PropertyChanged += dataChannel_PropertyChanged;
            }
            ClearResultsByRemovingTabs();
        }

        void dataChannel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ZedGraphManager.ZedGraphHelper.CreateGraph(ref graphControl, currentRecord);
        }


        private void AboutCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            
        }

        private void Calculate_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (currentRecord == null) return;
            lock (currentRecord)
            {
                RecordCalculation calc = new RecordCalculation(currentRecord);
                
                ChannelCalcFrequency calcFreq = calc.getFrequencyCalc();
                calcFreq.Initialize(Properties.Settings.Default.NumberOfPulses, 
                                    Properties.Settings.Default.NumberOfSmooth);
                if(calcFreq.Caclculate())
                {
                    
                    ChannelCalcPressure calcPressure = calc.getPressureCalc();
                    calcPressure.Initialize(
                        Properties.Settings.Default.StartPoint, 
                        Properties.Settings.Default.LengthSegment, 
                        Properties.Settings.Default.NumberOfPulses,Properties.Settings.Default.VerticalOffset,
                        Properties.Settings.Default.SensorConversionFactor,
                        Properties.Settings.Default.NumberOfSegments,
                        calcFreq.Fronts);
                    bool isPressureCalculated = calcPressure.Caclculate();
                    AddTabItemGraph(tagTabFrequency, "Частота вращения от времени", calcFreq.T_vu, calcFreq.Vu, "секунды", "об/мин", calcPressure.StartPointList.X, calcPressure.StartPointList.Y);
                    AddTabItemGraph(tagAccTime, "Ускорение от времени", calcFreq.T_ac, calcFreq.Ac, "секунды", "рад/с^2", calcPressure.StartPointList.X, calcPressure.StartPointList.Y);
                    
                    if(isPressureCalculated)
                    {
                        AddTabItemGraph(tagPressure, "Давление масла от угла поворота", calcPressure.Cycles, "градусы", "КПа");
                    }
                }

                

                
            }
            
        }

        private void ClearResultsByRemovingTabs()
        {
            List<TabItem> tabItemsToRemove = mainContentTab.Items.Cast<TabItem>().Where(i => (string)i.Tag != "dataSourceGraph").ToList();
            foreach(var tabItem in tabItemsToRemove){
                mainContentTab.Items.Remove(tabItem);
            }
        }

        private TabItem AddTabItemGraph(string tag, string name, double[] x, double[] y, string label_x, string label_y, double[] eventsX = null, double[] eventsY = null)
        {
            int count = mainContentTab.Items.Count;
            mainContentTab.Items.Remove(mainContentTab.Items.Cast<TabItem>().Where(i => i.Name == tag).SingleOrDefault());
            // create new tab item
            TabItem tab = new TabItem();
            tab.Header = name;
            tab.Name = tag;

            WindowsFormsHost hostZedGraph = new WindowsFormsHost();
            ZedGraphControl controlGraph = new ZedGraphControl();
            ZedGraphManager.ZedGraphHelper.CreateGraph(ref controlGraph, x, label_x, y, label_y, System.Drawing.Color.FromArgb(0, 240, 0), name, "");
            if (eventsX != null && eventsY != null)
            {
                for (int i = 0; i < eventsX.Count(); i++)
                    ZedGraphManager.ZedGraphHelper.AddNewPointToGraph(controlGraph, eventsX[i], eventsY[i]);
            }
            hostZedGraph.Child = controlGraph;
            tab.Content = hostZedGraph;
            mainContentTab.Items.Add(tab);
            return tab;
        }

        private TabItem AddTabItemGraph(string tag, string name, List<LGACurve> curves, string label_x, string label_y)
        {
            int count = mainContentTab.Items.Count;
            mainContentTab.Items.Remove(mainContentTab.Items.Cast<TabItem>().Where(i => i.Name == tag).SingleOrDefault());
            // create new tab item
            TabItem tab = new TabItem();
            tab.Header = name;
            tab.Name = tag;

            WindowsFormsHost hostZedGraph = new WindowsFormsHost();
            ZedGraphControl controlGraph = new ZedGraphControl();
            ZedGraphManager.ZedGraphHelper.CreateGraph(ref controlGraph, curves, label_x, label_y, System.Drawing.Color.FromArgb(0, 240, 0), name, "");
            hostZedGraph.Child = controlGraph;
            tab.Content = hostZedGraph;
            mainContentTab.Items.Add(tab);
            return tab;
        }

        private void Settings_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SettingsCalc calc = new SettingsCalc();
            calc.ShowDialog();
        } 

    }
}
