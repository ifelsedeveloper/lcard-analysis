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

namespace LGA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        LGraphData currentData;

        public MainWindow()
        {
            InitializeComponent();
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
            currentData = data;
            currentAction.Text = "Чтение данных завершено";
            dataGridFileProperty.ItemsSource = data.HeaderItems;
            dataGridChannelProperty.ItemsSource = data.DataChannels;
        }

        private void AboutCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            
        }


    }
}
