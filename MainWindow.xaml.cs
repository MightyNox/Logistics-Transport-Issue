using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Logistics_Transport_Issue
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Regex InputRegex = new Regex("[0-9]+");
        private const string BalancedTransportText = "Balanced transport detected";
        private const string UnbalancedTransportText = "Unbalanced transport detected - unsupported";

        private int[] _supply;
        private int[] _demand;
        private int[,] _costs;

        private int _producersCount;
        private int _receiversCount;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            _demand = new[] {20, 40, 90};
            _supply = new[] {50, 70, 30};
            _costs = new[,] {{3, 5, 7}, {12, 10, 9}, {13, 3, 9}};

            var demandSum = _demand.Sum();
            var supplySum = _supply.Sum();

            TransportBalanceLabel.Content = demandSum == supplySum ? BalancedTransportText : UnbalancedTransportText;

            Algorithm.Calculate(_supply, _demand, _costs);
        }

        private void ProducersCountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                _producersCount = int.Parse(ProducersCountTextBox.Text);
            }
            catch (Exception)
            {
                return;
            }

            ProducersCountTextBox.Text = _producersCount.ToString();
            PrintCanvasGrid();
        }

        private void ReceiversCountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                _receiversCount = int.Parse(ReceiversCountTextBox.Text);
            }
            catch (Exception)
            {
                return;
            }

            ReceiversCountTextBox.Text = _receiversCount.ToString();
            PrintCanvasGrid();
        }

        private void ProducersCountTextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !InputRegex.IsMatch(e.Text);
        }

        private void ReceiversCountTextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !InputRegex.IsMatch(e.Text);
        }

        private void CanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            PrintCanvasGrid();
        }
        private void PrintCanvasGrid()
        {
            if (TableCanvas == null) return;

            TableCanvas.Children.Clear();

            for (var row = 0; row < _producersCount + 1; row++)
            {
                var y = 1 + TableCanvas.ActualHeight / _producersCount * row;
                var line = new Line
                {
                    Stroke = Brushes.Black,
                    X1 = 1,
                    X2 = TableCanvas.ActualWidth,
                    Y1 = y,
                    Y2 = y,
                    StrokeThickness = 2
                };

                TableCanvas.Children.Add(line);
            }

            for (var column = 0; column < _receiversCount + 1; column++)
            {
                var x = 1 + TableCanvas.ActualWidth / _receiversCount * column;
                var line = new Line
                {
                    Stroke = Brushes.Black,
                    X1 = x,
                    X2 = x,
                    Y1 = 1,
                    Y2 = TableCanvas.ActualHeight,
                    StrokeThickness = 2
                };

                TableCanvas.Children.Add(line);
            }
        }
    }
}