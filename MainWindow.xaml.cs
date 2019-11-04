using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
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
        private const string UnbalancedTransportText = "Unbalanced transport detected";

        private int[] _supply;
        private int[] _demand;
        private int[,] _costs;

        private int _producersCount;
        private int _receiversCount;

        private TextBox[,] _costsTextBoxes;
        private TextBox[] _demandTextBoxes;
        private TextBox[] _supplyTextBoxes;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
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

            DisplayCostsCanvas();
            DisplayProducersCanvas();
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

            DisplayCostsCanvas();
            DisplayReceiversCanvas();
        }

        private void CostsTextBox_TextChanged(object sender, TextChangedEventArgs e, int row, int column)
        {
            int deliveryCosts;
            try
            {
                deliveryCosts = int.Parse(_costsTextBoxes[row, column].Text);
            }
            catch (Exception)
            {
                return;
            }

            _costs[row, column] = deliveryCosts;
            _costsTextBoxes[row, column].Text = deliveryCosts.ToString();
        }

        private void SupplyTextBox_TextChanged(object sender, TextChangedEventArgs e, int row)
        {
            int supply;
            try
            {
                supply = int.Parse(_supplyTextBoxes[row].Text);
            }
            catch (Exception)
            {
                return;
            }

            _supply[row] = supply;
            _supplyTextBoxes[row].Text = supply.ToString();
        }

        private void DemandTextBox_TextChanged(object sender, TextChangedEventArgs e, int column)
        {
            int demand;
            try
            {
                demand = int.Parse(_demandTextBoxes[column].Text);
            }
            catch (Exception)
            {
                return;
            }

            _demand[column] = demand;
            _demandTextBoxes[column].Text = demand.ToString();
        }

        private void TextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !InputRegex.IsMatch(e.Text);
        }

        private void CanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //TODO Split into 3 separate events
            DisplayCostsCanvas();
            DisplayReceiversCanvas();
            DisplayProducersCanvas();
        }

        private void DisplayCostsCanvas()
        {
            if (CostsCanvas == null) return;

            CostsCanvas.Children.Clear();

            //Print rows
            for (var row = 0; row < _producersCount + 1; row++)
            {
                var y = 1 + CostsCanvas.ActualHeight / _producersCount * row;
                var line = new Line
                {
                    Stroke = Brushes.Black,
                    X1 = 1,
                    X2 = CostsCanvas.ActualWidth,
                    Y1 = y,
                    Y2 = y,
                    StrokeThickness = 2
                };

                CostsCanvas.Children.Add(line);
            }

            //Print columns
            for (var column = 0; column < _receiversCount + 1; column++)
            {
                var x = 1 + CostsCanvas.ActualWidth / _receiversCount * column;
                var line = new Line
                {
                    Stroke = Brushes.Black,
                    X1 = x,
                    X2 = x,
                    Y1 = 1,
                    Y2 = CostsCanvas.ActualHeight,
                    StrokeThickness = 2
                };

                CostsCanvas.Children.Add(line);
            }

            _costsTextBoxes = new TextBox[_producersCount, _receiversCount];
            _costs = new int[_producersCount, _receiversCount];

            var columnWidth = CostsCanvas.ActualWidth / _receiversCount;
            var rowHigh = CostsCanvas.ActualHeight / _producersCount;

            //Print Costs TextBoxes
            for (var row = 0; row < _producersCount; row++)
            {
                for (var column = 0; column < _receiversCount; column++)
                {
                    var textBox = new TextBox();
                    var textBoxRow = row;
                    var textBoxColumn = column;

                    textBox.PreviewTextInput += TextBox_OnPreviewTextInput;
                    textBox.TextChanged += (sender, e) =>
                        CostsTextBox_TextChanged(sender, e, textBoxRow, textBoxColumn);
                    textBox.FontSize= 25;

                    textBox.Width = CostsCanvas.ActualWidth / _receiversCount - 5;
                    textBox.Height = CostsCanvas.ActualHeight / _producersCount - 5;

                    Canvas.SetLeft(textBox, 3  + columnWidth * column);
                    Canvas.SetTop(textBox, 3 + rowHigh * row);
                    CostsCanvas.Children.Add(textBox);

                    _costsTextBoxes[row, column] = textBox;
                }
            }
        }

        private void DisplayReceiversCanvas()
        {
            if (DemandCanvas == null) return;

            DemandCanvas.Children.Clear();

            _demandTextBoxes = new TextBox[_receiversCount];
            _demand = new int[_receiversCount];

            var columnWidth = DemandCanvas.ActualWidth / _receiversCount;

            //Print rows
            for (var row = 0; row < 2; row++)
            {
                var y = 1 + DemandCanvas.ActualHeight * row;
                var line = new Line
                {
                    Stroke = Brushes.Black,
                    X1 = 1,
                    X2 = DemandCanvas.ActualWidth,
                    Y1 = y,
                    Y2 = y,
                    StrokeThickness = 2
                };

                DemandCanvas.Children.Add(line);
            }

            //Print columns
            for (var column = 0; column < _receiversCount + 1; column++)
            {
                var x = 1 + DemandCanvas.ActualWidth / _receiversCount * column;
                var line = new Line
                {
                    Stroke = Brushes.Black,
                    X1 = x,
                    X2 = x,
                    Y1 = 1,
                    Y2 = DemandCanvas.ActualHeight,
                    StrokeThickness = 2
                };

                DemandCanvas.Children.Add(line);
            }

            //Print Demand TextBoxes
            for (var column = 0; column < _receiversCount; column++)
            {
                var textBox = new TextBox();
                var textBoxColumn = column;
            
                textBox.PreviewTextInput += TextBox_OnPreviewTextInput;
                textBox.TextChanged += (sender, e) => DemandTextBox_TextChanged(sender, e, textBoxColumn);
                textBox.FontSize = 25;

                textBox.Width = DemandCanvas.ActualWidth / _receiversCount - 5;
                textBox.Height = DemandCanvas.ActualHeight - 5;
            
                Canvas.SetLeft(textBox, 3 + columnWidth * column);
                Canvas.SetTop(textBox, 3);
                DemandCanvas.Children.Add(textBox);
            
                _demandTextBoxes[column] = textBox;
            }
        }

        private void DisplayProducersCanvas()
        {
            if (ProducersCanvas == null) return;

            ProducersCanvas.Children.Clear();

            _supplyTextBoxes = new TextBox[_producersCount];
            _supply = new int[_producersCount];

            var rowHigh = ProducersCanvas.ActualHeight / _producersCount;

            //Print columns
            for (var column = 0; column < 2; column++)
            {
                var x = 1 + ProducersCanvas.ActualWidth * column;
                var line = new Line
                {
                    Stroke = Brushes.Black,
                    X1 = x,
                    X2 = x,
                    Y1 = 1,
                    Y2 = ProducersCanvas.ActualHeight,
                    StrokeThickness = 2
                };

                ProducersCanvas.Children.Add(line);
            }

            //Print rows
            for (var row = 0; row < _producersCount + 1; row++)
            {
                var y = 1 + ProducersCanvas.ActualHeight / _producersCount * row;
                var line = new Line
                {
                    Stroke = Brushes.Black,
                    X1 = 1,
                    X2 = ProducersCanvas.ActualWidth,
                    Y1 = y,
                    Y2 = y,
                    StrokeThickness = 2
                };

                ProducersCanvas.Children.Add(line);
            }

            for (var row = 0; row < _producersCount; row++)
            {
                var textBox = new TextBox();
                var textBoxRow = row;

                textBox.PreviewTextInput += TextBox_OnPreviewTextInput;
                textBox.TextChanged += (sender, e) => SupplyTextBox_TextChanged(sender, e, textBoxRow);
                textBox.FontSize = 25;

                textBox.Width = ProducersCanvas.ActualWidth - 5;
                textBox.Height = ProducersCanvas.ActualHeight / _producersCount - 5;

                Canvas.SetLeft(textBox, 3);
                Canvas.SetTop(textBox, 3 + rowHigh * row);
                ProducersCanvas.Children.Add(textBox);

                _supplyTextBoxes[row] = textBox;
            }
        }
    }
}