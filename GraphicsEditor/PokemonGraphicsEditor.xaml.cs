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

namespace HGEngineHelper.GraphicsEditor
{
    /// <summary>
    /// Interaction logic for HGEImporterWindow.xaml
    /// </summary>
    public partial class PokemonGraphicsEditor : UserControl
    {
        public PokemonGraphicsEditor()
        {
            this.InitializeComponent();
        }

        private void btnToggleFrame_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnToggleShiny_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnToggleGender_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NumericTextbox_ValueChanged(object sender, RoutedEventArgs e)
        {
            var x = 1;
        }

        private void NumericTextbox_ValueChangedEvent(object sender, EventArgs e)
        {
            var x = 1;
        }

        private void NumericTextbox_ValueChangedEvent_1(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Changed", "", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cboShadowSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
