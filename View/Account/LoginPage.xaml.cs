using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using AI_Times.Data;
using System.Linq;

namespace AI_Times.View.Account
{
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var email = EmailBox.Text;
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ErrorText.Text = "Please enter both email and password.";
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            using var db = new AppDbContext();
            
            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ErrorText.Visibility = Visibility.Collapsed;
                App.LoggedInUser = user;
                // Redirect on success
                Frame.Navigate(typeof(Home.HomePage));
            }
            else
            {
                ErrorText.Text = "Invalid email or password.";
                ErrorText.Visibility = Visibility.Visible;
            }
        }
        
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RegistrationPage));
        }

        private void BackHomeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Home.HomePage));
        }
    }
}