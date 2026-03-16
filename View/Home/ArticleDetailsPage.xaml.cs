using AI_Times.Data.Models;
using AI_Times.Data;
using AI_Times.Data.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AI_Times.View.Home
{
    public sealed partial class ArticleDetailsPage : Page
    {
        public NewspaperArticle? Article { get; private set; }

        public ArticleDetailsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is NewspaperArticle article)
            {
                Article = article;
                Bindings.Update();
                UpdateVerifyButtonVisibility();
            }
        }

        private void UpdateVerifyButtonVisibility()
        {
            if (Article != null && App.LoggedInUser != null && App.LoggedInUser.Role == "Admin" && !Article.Verified)
            {
                VerifyButton.Visibility = Visibility.Visible;
            }
            else
            {
                VerifyButton.Visibility = Visibility.Collapsed;
            }
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            if (Article != null && App.LoggedInUser != null && App.LoggedInUser.Role == "Admin")
            {
                using var db = new AppDbContext();
                var dbArticle = db.Articles.Find(Article.Id);
                if (dbArticle != null)
                {
                    dbArticle.Verified = true;
                    dbArticle.VerifiedBy = App.LoggedInUser.Id;
                    db.SaveChanges();
                    
                    Article.Verified = true;
                    Article.VerifiedBy = App.LoggedInUser.Id;
                    
                    UpdateVerifyButtonVisibility();
                    Bindings.Update();
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame?.CanGoBack == true)
            {
                Frame.GoBack();
            }
        }
    }
}
