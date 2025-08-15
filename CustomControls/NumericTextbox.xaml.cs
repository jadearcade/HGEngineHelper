using HgEngineCsvConverter.Code;
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

namespace HGEngineHelper.CustomControls
{
    /// <summary>
    /// Interaction logic for NumericTextbox.xaml
    /// </summary>
    public partial class NumericTextbox : UserControl
    {
        public NumericTextbox()
        {
            InitializeComponent();
        }

        public double LabelWidth
        {
            get
            {
                return lblAttribute.Width;
            }
            set
            {
                lblAttribute.Width = value;
            }
        }

        public string Label
        {
            get
            {
                return lblAttribute.Text; ;
            }
            set
            {
                lblAttribute.Text = value;
            }
        }

        public static readonly RoutedEvent ValueChanged = EventManager.RegisterRoutedEvent(
            "ValueChangedEvent", RoutingStrategy.Bubble
            , typeof(RoutedEventHandler), typeof(NumericTextbox));

        public event RoutedEventHandler? ValueChangedEvent
        {
            add { AddHandler(ValueChanged, value); }
            remove { RemoveHandler(ValueChanged, value); }
        }

        private void RaiseValueChangedEvent()
        {
            if (__ValueWhenLastRaisedEvent.HasValue && __ValueWhenLastRaisedEvent == Value)
            {
                return;
            }
            RoutedEventArgs args = new RoutedEventArgs(ValueChanged, this);
            __ValueWhenLastRaisedEvent = Value;
            RaiseEvent(args);
        }

        private int? __ValueWhenLastRaisedEvent;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var intValue = txtValue.Text.ToInt();
            if (intValue == 0 && txtValue.Text != "0")
            {
                txtValue.Text = "0";
            }
            Value = intValue;
            RaiseValueChangedEvent();
        }

        public int Value
        {
            get
            {
                return txtValue.Text.ToInt();
            }
            set
            {
                int newValue = HelperFunctions.RestrainToMinAndMax(value, MinValue, MaxValue);
                txtValue.Text = newValue.ToString();
                RaiseValueChangedEvent();
            }
        }

        public int? MinValue = null;
        public int? MaxValue = null;

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                IncrementValue();
            }
            else if (e.Key == Key.Down)
            {
                DecrementValue();
            }
        }

        private void IncrementValue()
        {
            Value = Value + 1;
        }

        private void DecrementValue()
        {
            Value = Value - 1;
        }

        private void btnDownValue_Click(object sender, RoutedEventArgs e)
        {
            DecrementValue();
        }

        private void btnUpValue_Click(object sender, RoutedEventArgs e)
        {
            IncrementValue();
        }
    }
}
