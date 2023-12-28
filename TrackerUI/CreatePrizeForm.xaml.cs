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
using TrackerLibrary.Models;
using TrackerLibrary;
using MessageBox = System.Windows.MessageBox;

namespace TrackerUI
{
    /// <summary>
    /// Interaction logic for CreatePrizeForm.xaml
    /// </summary>
    public partial class CreatePrizeForm : Window
    {
        private IPrizeRequester callingForm;
        public CreatePrizeForm(IPrizeRequester caller)
        {
            InitializeComponent();
            callingForm = caller;
        }

        private void createPrizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateForm())
            {
                var prize = new PrizeModel(
                    placeNumberValue.Text,
                    placeNameValue.Text,
                    prizeAmountValue.Text,
                    prizePercentageValue.Text);

                GlobalConfig.Connection.CreatePrize(prize);
                callingForm.PrizeComplete(prize);

                this.Close();
            }
            else
            {
                MessageBox.Show("This form has invalid information.");
            }
        }

        private bool ValidateForm()
        {
            var isPlaceNumberValid = int.TryParse(placeNumberValue.Text, out var placeNumber);
            var isPrizeAmountValid = decimal.TryParse(prizeAmountValue.Text, out var prizeAmount);
            var isPrizePercentageValid = int.TryParse(prizePercentageValue.Text, out var prizePercentage);

            return (isPlaceNumberValid && placeNumber >= 1 &&
                    placeNameValue.Text.Length > 0 &&
                    isPrizeAmountValid && prizeAmount >= 0 &&
                    isPrizePercentageValid && prizePercentage is >= 0 and <= 100);
        }
    }
}
