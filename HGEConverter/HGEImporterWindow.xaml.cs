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
using System.Windows.Shapes;
using static HGEngineHelper.HGEConverter.HGEConverterController;

namespace HGEngineHelper.HGEConverter
{
    /// <summary>
    /// Interaction logic for HGEImporterWindow.xaml
    /// </summary>
    public partial class HGEImporterWindow : Window, HGEImportProgress
    {
        public HGEImporterWindow()
        {
            InitializeComponent();
            //(new HGEConverterController()).UpdateProjectDataFromHgEngine(this);
        }
        //public async Task ProgressTest(HGEImportProgress progress)
        //{
        //    for(int i = 0; i < 100; i++)
        //    {
        //        Thread.Sleep(50);
        //        progress.Report(new ProgressInfo(i, i + "%"));
        //    }
        //}

        public void Report(ProgressInfo value)
        {
            Dispatcher.Invoke(() =>
            {
                lblProgressMessage.Text = value.Message;
                if (value.Value == 0)
                {
                    progressBar.Visibility = Visibility.Collapsed;
                }
                else if (value.Value == 100)
                {
                    lblProgressMessage.Text = value.Message;
                    progressBar.Visibility = Visibility.Collapsed;
                }
                else
                {
                    lblProgressMessage.Visibility = Visibility.Visible;
                    progressBar.Visibility = Visibility.Visible;
                    progressBar.Value = value.Value;
                }
            });
        }

        private async void btnStartImport_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => new HGEConverterController().UpdateProjectDataFromHgEngine(this));
        }
    }
}
