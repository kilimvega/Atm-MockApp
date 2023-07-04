using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Npgsql;

namespace Atm_mock_app
{
    public partial class MainWindow : Window
    {
        public int MainAccountMoney;
        public int NewBalance;
        public string selectedAccount;

        public MainWindow()
        {
            // this part here makes sure that only the needed overlays are visible at the start
            InitializeComponent();
            StartOverlay.Visibility = Visibility.Visible;
            PasswordOverlay.Visibility = Visibility.Visible;
            MainOverlay.Visibility = Visibility.Hidden;
            CurrentBalanceOverlay.Visibility = Visibility.Hidden;
            WithdrawAmountOverlay.Visibility = Visibility.Hidden;
            SuccessfulWithdrawOverlay.Visibility = Visibility.Hidden;
            FailedWithdrawOverlay.Visibility = Visibility.Hidden;
        }

        //this function takes care of opening a connection to the database to check if the user input matches any existing accounts in the postgresql database
        private void accountNumber_KeyDown( object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var ConnectionString = "Server=localhost;Port=5432;Database=AtmMockAppDatabase;User Id=postgres;Password=13may2019;";
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();

                    var commandText = "SELECT * FROM users";
                    using (var command = new NpgsqlCommand(commandText, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var column1value = reader["account_number"];
                                var column2value = reader["account_pin"];
                                var column3value = reader["account_balance"];

                                var userInput = accountNumberTextBox.Text;
                                if (userInput == column1value.ToString())
                                {
                                    StartOverlay.Visibility = Visibility.Hidden;
                                    PasswordOverlay.Visibility = Visibility.Visible;
                                    selectedAccount = userInput.ToString();
                                    WrongAccountText.Visibility = Visibility.Hidden;
                                }
                                else if (userInput != column1value.ToString())
                                {
                                    WrongAccountText.Visibility = Visibility.Visible;
                                }
                            }
                        }
                    }
                    connection.Close();
                }
            }
        }

        // this function checks the user input to see if the password is correct
        public void Window_PinInput(object sender, KeyEventArgs e)
        {
            if (((TextBox)sender).Text.Length == ((TextBox)sender).MaxLength)
            {
                var ConnectionString = "Server=localhost;Port=5432;Database=AtmMockAppDatabase;User Id=postgres;Password=13may2019;";
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();

                    var commandText = "SELECT * FROM users WHERE account_number = @selectedAccount";
                    using (var command = new NpgsqlCommand(commandText, connection))
                    {
                        command.Parameters.AddWithValue("selectedAccount", selectedAccount);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var column1value = reader["account_number"];
                                var column2value = reader["account_pin"];
                                var column3value = reader["account_balance"];

                                var userInput = ((TextBox)sender).Text;
                                if (userInput == column2value.ToString())
                                {
                                    PasswordOverlay.Visibility = Visibility.Hidden;
                                    MainOverlay.Visibility = Visibility.Visible;
                                    WrongPinText.Visibility = Visibility.Hidden;
                                    ((TextBox)sender).Clear();
                                    int column3toInt = int.Parse(column3value.ToString());
                                    MainAccountMoney = column3toInt;
                                }
                                else if (userInput != column2value.ToString())
                                {
                                    WrongPinText.Visibility = Visibility.Visible;
                                    ((TextBox)sender).Clear();
                                }
                            }
                        }
                    }
                    connection.Close();
                }
            }
        }

        // this function hides and shows the proper overlays when the user clicks on the exit button located in the main menu
        public void Window_ExitButtonClick(object sender, MouseEventArgs e)
        {
            if (MainOverlay.Visibility == Visibility.Visible)
            {
                PasswordOverlay.Visibility = Visibility.Visible;
                StartOverlay.Visibility = Visibility.Visible;
                MainOverlay.Visibility = Visibility.Hidden;
                accountNumberTextBox.Clear();
                WrongAccountText.Visibility = Visibility.Hidden;
            }
        }

        // this function hides and shows the proper overlays when the user clicks on the exit button located in the balance menu
        public void Window_ExitBalanceClick(object sender, MouseEventArgs e)
        {
            if (CurrentBalanceOverlay.Visibility == Visibility.Visible)
            {
                MainOverlay.Visibility = Visibility.Visible;
                CurrentBalanceOverlay.Visibility = Visibility.Hidden;
            }
        }

        // this function hides and shows the proper overlays when the user clicks on the exit button located in the withdraw menu
        public void Window_ExitWithdrawClick(object sender, MouseEventArgs e)
        {
            if (WithdrawAmountOverlay.Visibility == Visibility.Visible)
            {
                MainOverlay.Visibility = Visibility.Visible;
                WithdrawAmountOverlay.Visibility = Visibility.Hidden;
            }
        }

        // this function hides and shows the proper overlays when the user clicks on the balance button in the main menu
        public void Window_BalanceButtonClick(object sender, MouseEventArgs e)
        {
            if (MainOverlay.Visibility == Visibility.Visible)
            {
                MainOverlay.Visibility = Visibility.Hidden;
                CurrentBalanceOverlay.Visibility = Visibility.Visible;

                CurrentBalance.Text = MainAccountMoney.ToString();
            }
        }

        // this function hides and shows the proper overlays when the user clicks on the withdraw button in the main menu
        public void Window_WithdrawButtonClick(object sender, MouseEventArgs e)
        {
            if (MainOverlay.Visibility == Visibility.Visible)
            {
                MainOverlay.Visibility = Visibility.Hidden;
                WithdrawAmountOverlay.Visibility = Visibility.Visible;
                WithdrawCustom.Text = "Please Enter Custom Amount and Press Enter";
            }
        }
        
        // this function creates a delay that closes the overlay that appears after making a successful or failed withdraw
        public async Task CloseOverlay()
        {
            if (SuccessfulWithdrawOverlay.Visibility == Visibility.Visible)
            {
                await Task.Delay(1000);
                SuccessfulWithdrawOverlay.Visibility = Visibility.Hidden;
                PasswordOverlay.Visibility = Visibility.Visible;
                StartOverlay.Visibility = Visibility.Visible;
                WrongAccountText.Visibility = Visibility.Hidden;
                MainOverlay.Visibility = Visibility.Hidden;
                accountNumberTextBox.Clear();
            }
            else if (FailedWithdrawOverlay.Visibility == Visibility.Visible)
            {
                await Task.Delay(1000);
                FailedWithdrawOverlay.Visibility = Visibility.Hidden;
                PasswordOverlay.Visibility = Visibility.Visible;
                StartOverlay.Visibility = Visibility.Visible;
                MainOverlay.Visibility = Visibility.Hidden;
                accountNumberTextBox.Clear();
            }
        }

        // these next 5 functions all take care of withdrawing the amount specified in the button thats being clicked
        public async void Window_Withdraw20(object sender, MouseEventArgs e)
        {
            if(MainAccountMoney >= 20)
            {
                MainAccountMoney -= 20;
                WithdrawAmountOverlay.Visibility = Visibility.Hidden;
                SuccessfulWithdrawOverlay.Visibility = Visibility.Visible;
                NewBalanceTextBox.Text = $"Your New Balance Is: {MainAccountMoney}";
                await CloseOverlay();
            }
            else if (MainAccountMoney < 20)
            {
                WithdrawAmountOverlay.Visibility = Visibility.Hidden;
                FailedWithdrawOverlay.Visibility = Visibility.Visible;
                await CloseOverlay();
            }
        }

        public async void Window_Withdraw40(object sender, MouseEventArgs e)
        {
            if (MainAccountMoney >= 40)
            {
                MainAccountMoney -= 40;
                WithdrawAmountOverlay.Visibility = Visibility.Hidden;
                SuccessfulWithdrawOverlay.Visibility = Visibility.Visible;
                NewBalanceTextBox.Text = $"Your New Balance Is: {MainAccountMoney}";
                await CloseOverlay();
            }
            else if (MainAccountMoney < 40)
            {
                WithdrawAmountOverlay.Visibility = Visibility.Hidden;
                FailedWithdrawOverlay.Visibility = Visibility.Visible;
                await CloseOverlay();
            }
        }

        public async void Window_Withdraw60(object sender, MouseEventArgs e)
        {
            if (MainAccountMoney >= 60)
            {
                MainAccountMoney -= 60;
                WithdrawAmountOverlay.Visibility = Visibility.Hidden;
                SuccessfulWithdrawOverlay.Visibility = Visibility.Visible;
                NewBalanceTextBox.Text = $"Your New Balance Is: {MainAccountMoney}";
                await CloseOverlay();
            }
            else if (MainAccountMoney < 60)
            {
                WithdrawAmountOverlay.Visibility = Visibility.Hidden;
                FailedWithdrawOverlay.Visibility = Visibility.Visible;
                await CloseOverlay();
            }
        }
        public async void Window_Withdraw100(object sender, MouseEventArgs e)
        {
            if (MainAccountMoney >= 100)
            {
                MainAccountMoney -= 100;
                WithdrawAmountOverlay.Visibility = Visibility.Hidden;
                SuccessfulWithdrawOverlay.Visibility = Visibility.Visible;
                NewBalanceTextBox.Text = $"Your New Balance Is: {MainAccountMoney}";
                await CloseOverlay();
            }
            else if (MainAccountMoney < 100)
            {
                WithdrawAmountOverlay.Visibility = Visibility.Hidden;
                FailedWithdrawOverlay.Visibility = Visibility.Visible;
                await CloseOverlay();
            }
        }
        public async void Window_Withdraw200(object sender, MouseEventArgs e)
        {
            if (MainAccountMoney >= 200)
            {
                MainAccountMoney -= 200;
                WithdrawAmountOverlay.Visibility = Visibility.Hidden;
                SuccessfulWithdrawOverlay.Visibility = Visibility.Visible;
                NewBalanceTextBox.Text = $"Your New Balance Is: {MainAccountMoney}";
                await CloseOverlay();
            }
            else if (MainAccountMoney < 200)
            {
                WithdrawAmountOverlay.Visibility = Visibility.Hidden;
                FailedWithdrawOverlay.Visibility = Visibility.Visible;
                await CloseOverlay();
            }
        }

        // this is what makes the textbox in the withdraw screen accept only numbers
        private void Window_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // this basic function takes care of cleaning the texboxes when clicked on them
        private void Window_ClickClearBox(object sender, MouseEventArgs e)
        {
            ((TextBox)sender).Clear();
        }

        // this function performs a withdrawal with the user input amount
        public async void Window_WithdrawCustom(object sender, KeyEventArgs e)
        {
            string customAmount = ((TextBox)sender).Text.ToString();
            if (e.Key == Key.Enter)
            {
                if (MainAccountMoney >= int.Parse(customAmount))
                {
                    MainAccountMoney -= int.Parse(customAmount);
                    WithdrawAmountOverlay.Visibility = Visibility.Hidden;
                    SuccessfulWithdrawOverlay.Visibility = Visibility.Visible;
                    NewBalanceTextBox.Text = $"Your New Balance Is: {MainAccountMoney}";
                    await CloseOverlay();
                }
                else if (MainAccountMoney < int.Parse(customAmount))
                {
                    WithdrawAmountOverlay.Visibility = Visibility.Hidden;
                    FailedWithdrawOverlay.Visibility = Visibility.Visible;
                    await CloseOverlay();
                }
            }
        }
    }

}
