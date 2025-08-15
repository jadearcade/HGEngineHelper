using CsvHelper;
using HgEngineCsvConverter;
using HGEngineHelper.Code.CsvProcessing;
using HGEngineHelper.Code.CsvProcessing.Models;
using HGEngineHelper.Code.HGECodeHelper;
using HGEngineHelper.Code.HGECodeHelper.Settings;
using HGEngineHelper.Code.HGEngineExport;
using HGEngineHelper.Code.HGEngineImport;
using HGEngineHelper.GraphicsEditor;
using HGEngineHelper.HGEConverter;
using HGEngineHelper.Project;
using System.Collections.ObjectModel;
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
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MyPokemonGrid.ItemsSource = new MyViewModel().MyDataCollection;
            Title = "HGEngine Helper - " + App.ProjectInfo?.name ?? "No Project";
            //newProjectWindow.Owner = this;
        }

        public class MyViewModel
        {
            public List<MonInfo> MyDataCollection { get; set; } = new List<MonInfo>();

            public MyViewModel() // Or in your Window/UserControl's code-behind
            {
                //int count = 100;
                try
                {
                    var pokemon = (new HGEHelperCsvReader()).GetPokemonForProject(App.ProjectInfo);
                    //pokemon.RemoveRange(count, pokemon.Count - count);
                    MyDataCollection = pokemon;
                }catch(Exception e)
                {

                }   
            }
        }

        private void MenuNewProject_Click(object sender, RoutedEventArgs e)
        {
            (new CreateNewProjectWindow()).Show();
        }

        private void MenuOpenProject_Click(object sender, RoutedEventArgs e)
        {
            (new OpenProjectController()).OpenOpenProjectDialog(this);
        }

        private void MenuPokemon_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ImportFromHge_Click(object sender, RoutedEventArgs e)
        {
            (new HGEImporterWindow()).Show();
        }
        private void MenuGraphics_Click(object sender, RoutedEventArgs e)
        {
            (new PokemonGraphicsEditorWindow()).Show();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuSettings_Click(object sender, RoutedEventArgs e)
        {
            var importer = new ImporterFromHge();
            importer.ImportPokemonDataAndCodeFromHge();
            importer.ImportEvolutionData();
            importer.ImportFormAndMegaData();
            importer.ImportOverworldData();
        }

        private void MenuWriteCode_Click(object sender, RoutedEventArgs e)
        {
            var exporter = new ExporterToHge();
            exporter.ExportPokemonDataToHgeTestFiles();
            exporter.ExportEvoDataToHgeTestFiles();
        }
    }
}