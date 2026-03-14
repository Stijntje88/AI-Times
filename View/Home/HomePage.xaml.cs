using Microsoft.UI.Xaml;
using AI_Times.Data;
using AI_Times.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AI_Times.View.Home
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public ObservableCollection<NewspaperArticle> Articles { get; set; }

        public HomePage()
        {
            InitializeComponent();
            Articles = new ObservableCollection<NewspaperArticle>();
            LoadArticles();
        }

        private async void LoadArticles()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var articles = await db.Articles
                        .OrderByDescending(a => a.PublishDate)
                        .ToListAsync();

                    Articles.Clear();
                    foreach (var article in articles)
                    {
                        Articles.Add(article);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading articles: {ex.Message}");
            }
        }

        private void ArticlesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is NewspaperArticle article)
            {
                Frame?.Navigate(typeof(ArticleDetailsPage), article);
            }
        }

        private void GenreButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string genre)
            {
                Frame?.Navigate(typeof(GenrePage), genre);
            }
        }

        private void ArticleCard_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Grid grid)
            {
                grid.BorderThickness = new Microsoft.UI.Xaml.Thickness(2);
            }
        }

        private void ArticleCard_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Grid grid)
            {
                grid.BorderThickness = new Microsoft.UI.Xaml.Thickness(1);
            }
        }
    }
}

