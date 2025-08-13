using System;
using Microsoft.Maui.Controls;
using TokeroApp.Services;

namespace TokeroApp
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void OnLoginClicked(object sender, EventArgs e)
        {
            string username = usernameEntry.Text;
            string password = passwordEntry.Text;

            if (IsValidLogin(username, password))
            {
                // Navigate to MainPage or HomePage
                Application.Current.MainPage = new NavigationPage(new DcaCalculatorPage());
            }
            else
            {
                messageLabel.Text = "Invalid credentials. Please try again.";
                messageLabel.IsVisible = true;
            }
        }

        private bool IsValidLogin(string username, string password)
        {
            return username == "admin" && password == "password";
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var service = new CryptoPriceService();
            var fetcher = new PriceFetcher(service, App.Database);
            await fetcher.FetchAndStoreAsync();
        }
    }
}