using HGEngineHelper.Code.HGECodeHelper.Settings;
using HGEngineHelper.Project;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
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
using Newtonsoft.Json;

namespace HGEngineHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class CreateNewProjectWindow : Window
    {
        private ConfigurationController configController = new ConfigurationController();
        public CreateNewProjectWindow()
        {
            InitializeComponent();
            txtProjectLocation.Text = configController.GetDefaultProjectsFolderLocation();
            txtHgEngineLocation.Text = configController.GetDefaultFileLocation();
            //MyPokemonGrid.ItemsSource = new MyViewModel().MyDataCollection;
        }

        private void ButtonCreateHgeHelperProject_Click(object sender, RoutedEventArgs e)
        {
            string name = txtProjectName.Text.ToString();
            string newDefaultProjectLocation = txtProjectLocation.Text.ToString();
            string newFolderName = newDefaultProjectLocation + "\\" + name;
            Directory.CreateDirectory(newFolderName);
            HGEngineHelperProjectInfo newProjectInfo = new HGEngineHelperProjectInfo()
            {
                hgEnginePath = txtHgEngineLocation.Text.ToString(),
                name = name,
                pathToProjectFolder = newFolderName,
            };
            newProjectInfo.Save();
            bool openProjectResult = (new OpenProjectController()).OpenProject(newProjectInfo);
            if (openProjectResult)
            {
                Close();
                (new MainWindow()).Show();
            }
        }

        private void ButtonOpenHgEngineFolderDialog_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();

            // Optional: Configure the dialog
            openFolderDialog.Title = "Select hg-engine folder";
            openFolderDialog.DefaultDirectory = txtProjectLocation.Text;
            openFolderDialog.Multiselect = false;

            // Show the dialog and check if the user clicked OK
            bool? result = openFolderDialog.ShowDialog();
            if (result.HasValue ? !result.Value : true)
            {
                return;
            }
            txtHgEngineLocation.Text = openFolderDialog.FolderName;
        }

        private void ButtonOpenProjectDirectoryFolderDialog_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();

            // Optional: Configure the dialog
            openFolderDialog.Title = "Select project folder";
            openFolderDialog.DefaultDirectory = txtProjectLocation.Text;
            openFolderDialog.Multiselect = false;

            // Show the dialog and check if the user clicked OK
            bool? result = openFolderDialog.ShowDialog();
            if (result.HasValue ? !result.Value : true)
            {
                return;
            }
            txtProjectLocation.Text = openFolderDialog.FolderName;
        }
    }
}