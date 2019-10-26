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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            //_demand = new[] { 20, 40, 90 };
            //_supply = new[] { 50, 70, 30 };
            //_costs = new[,] { { 3, 5, 7 }, { 12, 10, 9 }, { 13, 3, 9 } };

            _demand = new[] { 20, 40, 90 };
            _supply = new[] { 50, 70, 30 };
            _costs = new[,] { { 3, 5, 7 }, { 12, 10, 9 }, { 13, 3, 9 } };

            var demandSum = _demand.Sum();
            var supplySum = _supply.Sum();

            transportBalanceLabel.Content = demandSum == supplySum ? BalancedTransportText : UnbalancedTransportText;

            Algorithm.Calculate(_supply, _demand, _costs);
        }

        private void ProducersCountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            dynamic producersCount = int.Parse(producersCountTextBox.Text);
            producersCountTextBox.Text = producersCount.ToString();
        }

        private void ReceiversCountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            dynamic receiversCount = int.Parse(producersCountTextBox.Text);
            producersCountTextBox.Text = receiversCount.ToString();
        }

        private void ProducersCountTextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !InputRegex.IsMatch(e.Text);
        }

        private void ReceiversCountTextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !InputRegex.IsMatch(e.Text);
        }
    }
}
