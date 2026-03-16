using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using AI_Times.Data.Models;
using System.Collections.ObjectModel;
using AI_Times.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;

namespace AI_Times.View.Home
{
    public sealed partial class GenrePage : Page
    {
        public ObservableCollection<NewspaperArticle> Articles { get; set; }
        private string _genre;

        public GenrePage()
        {
            InitializeComponent();
            Articles = new ObservableCollection<NewspaperArticle>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string genre)
            {
                _genre = genre;
                GenreTitleText.Text = genre;
                try
                {
                    using (var db = new AppDbContext())
                    {
                        var query = db.Articles.Where(a => a.Genre == genre);

                        if (App.LoggedInUser == null)
                        {
                            // Ensure visitors don't see verified articles
                            query = query.Where(a => !a.Verified);
                        }

                        var articles = await query
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
                    System.Diagnostics.Debug.WriteLine($"Error loading articles for genre: {ex.Message}");
                }
            }
        }

        private void ArticlesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is NewspaperArticle article)
            {
                Frame?.Navigate(typeof(ArticleDetailsPage), article);
            }
        }

        private void BackButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }
}
