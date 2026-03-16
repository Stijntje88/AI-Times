using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using AI_Times.Data;
using AI_Times.Data.Models;
using System.Linq;

namespace AI_Times.View.Account
{
    public sealed partial class RegistrationPage : Page
    {
        public RegistrationPage()
        {
            this.InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var name = NameBox.Text;
            var email = EmailBox.Text;
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ErrorText.Text = "Please fill in all fields.";
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            using var db = new AppDbContext();
            
            // Check if user exists
            if (db.Users.Any(u => u.Email == email))
            {
                ErrorText.Text = "Email already in use.";
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            // Create new user (Hash password using BCrypt securely in real app, here pseudo-hashed)
            var newUser = new User
            {
                Name = name,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "User" // Default role
            };

            db.Users.Add(newUser);
            db.SaveChanges();
            
            ErrorText.Visibility = Visibility.Collapsed;

            // Navigate back to login
            Frame.Navigate(typeof(LoginPage));
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }

        private void BackHomeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Home.HomePage));
        }
    }
}
