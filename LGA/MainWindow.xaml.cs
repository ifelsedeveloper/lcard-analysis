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
using DevExpress.Charts;
using DevExpress.Xpf.Charts;
using System.Data;
using System.Collections.ObjectModel;
using DevExpress.Xpf.Core;
using System.Windows.Forms.Integration;
using DevExpress.XtraCharts;
using ZedGraph;

namespace LGA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        LGraphData currentRecord;
        ZedGraphControl graphControl = new ZedGraphControl();
        public MainWindow()
        {
            InitializeComponent();
            graphHost.Child = graphControl;
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
                    dataManager.ReadDataAsync(filename, new Action<LGraphData>(data =>
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            newFileOpened(data);
                        }));

                    }));
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
            ZedGraphHelper.ZedGraphHelper.CreateGraph(ref graphControl, data.DataChannels);
            foreach (var dataChannel in data.DataChannels)
            {
                dataChannel.PropertyChanged += dataChannel_PropertyChanged;
            }
        }

        void dataChannel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ZedGraphHelper.ZedGraphHelper.CreateGraph(ref graphControl, currentRecord.DataChannels);
        }


        private void AboutCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            
        }


    }
}
