using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using AI_Times.Data;
using AI_Times.View.Home;
using Microsoft.UI.Windowing;
using WinRT.Interop;

namespace AI_Times
{
    public sealed partial class MainWindow : Window
    {
        private bool _isInitialized;

        public MainWindow()
        {
            this.InitializeComponent();
            SetWindowIcon();
            this.Activated += MainWindow_Activated;
        }

        private void SetWindowIcon()
        {
            var hWnd = WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.SetIcon("Assets\\AI-Times-Logo - Outside-App.png");
        }

        private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState != WindowActivationState.Deactivated && !_isInitialized)
            {
                _isInitialized = true;
                this.Activated -= MainWindow_Activated;
                
                try
                {
                    Console.WriteLine("Starting database initialization...");
                    await InitializeDatabaseAsync();
                    Console.WriteLine("Database initialization completed.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during initialization: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
                finally
                {
                    if (contentFrame.Content is not HomePage)
                    {
                        contentFrame.Navigate(typeof(HomePage));
                    }
                }
            }
        }

        private async Task InitializeDatabaseAsync()
        {
            using var db = new AppDbContext();
            db.Database.EnsureDeleted();//do not touch this line, it is for testing purposes only, it will delete the database every time the app starts, so we can test the GenerateDailyNewsAsync method without having to worry about existing data
            Console.WriteLine("Ensuring database is created...");
            db.Database.EnsureCreated();

            Console.WriteLine("Calling GenerateDailyNewsAsync...");
            await AppDbContext.GenerateDailyNewsAsync(db);
            
            Console.WriteLine("Checking articles count...");
            var count = db.Articles.Count();
            Console.WriteLine($"Total articles in database: {count}");
        }
    }
}