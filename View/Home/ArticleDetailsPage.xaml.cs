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
