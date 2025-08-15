using HGEngineHelper.Project;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HGEngineHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
            var autoOpen = new ConfigurationController().GetAutoOpenLastProjectOnOpen();
            if (autoOpen)
            {
                //TODO: Debugging having issues with Configuration so hardcoded right now
                (new OpenProjectController()).OpenProjectFromFile("C:\\RomHack\\HGEHelperProjects\\TestProject\\TestProject.hgeh");
                (new MainWindow()).Show();
                Close();
            }
            //MyPokemonGrid.ItemsSource = new MyViewModel().MyDataCollection;
        }

        private void btnNewProject_Click(object sender, RoutedEventArgs e)
        {
            (new CreateNewProjectWindow()).Show();
        }

        private void btnOpenProject_Click(object sender, RoutedEventArgs e)
        {
            (new OpenProjectController()).OpenOpenProjectDialog(this);
        }
    }
}