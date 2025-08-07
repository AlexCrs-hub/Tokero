using System;
using Microsoft.Maui.Controls;

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
                Application.Current.MainPage = new MainPage();
            }
            else
            {
                messageLabel.Text = "Invalid credentials. Please try again.";
                messageLabel.IsVisible = true;
            }
        }

        private bool IsValidLogin(string username, string password)
        {
            // Replace this with real authentication logic
            return username == "admin" && password == "password";
        }
    }
}