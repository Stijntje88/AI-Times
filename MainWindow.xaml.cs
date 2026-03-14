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
        private double _targetProgress;
        private double _currentProgress;
        private string _currentMessage = "Starting up...";
        private DispatcherTimer _progressTimer;

        public MainWindow()
        {
            this.InitializeComponent();
            SetWindowIcon();
            this.Activated += MainWindow_Activated;

            _progressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            _progressTimer.Tick += ProgressTimer_Tick;
            _progressTimer.Start();
        }

        private void ProgressTimer_Tick(object sender, object e)
        {
            if (_currentProgress < _targetProgress)
            {
                // Smoothly ease toward the target progress
                double diff = _targetProgress - _currentProgress;
                double step = diff * 0.02; // ease factor
                
                // Keep a slow minimum crawling speed so it never feels stalled
                if (step < 0.1) step = 0.1; 

                _currentProgress += step;
                if (_currentProgress > _targetProgress)
                {
                    _currentProgress = _targetProgress;
                }

                if (LoadingProgressBar != null)
                {
                    LoadingProgressBar.Value = _currentProgress;
                }
                if (LoadingText != null)
                {
                    LoadingText.Text = $"{(int)_currentProgress}% - {_currentMessage}";
                }
            }
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
                    LoadingOverlay.Visibility = Visibility.Collapsed;
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
            UpdateProgress(10, "Resetting database...");
            db.Database.EnsureDeleted();//do not touch this line, it is for testing purposes only, it will delete the database every time the app starts, so we can test the GenerateDailyNewsAsync method without having to worry about existing data
                        
            UpdateProgress(30, "Ensuring database is created...");
            Console.WriteLine("Ensuring database is created...");
            db.Database.EnsureCreated();

            // Set target high so the bar creeps up smoothly during the heavy API call
            UpdateProgress(90, "Generating daily news...");
            Console.WriteLine("Calling GenerateDailyNewsAsync...");
            await AppDbContext.GenerateDailyNewsAsync(db);
            
            UpdateProgress(95, "Verifying articles...");
            Console.WriteLine("Checking articles count...");
            var count = db.Articles.Count();
            Console.WriteLine($"Total articles in database: {count}");
            
            UpdateProgress(100, "Ready!");
            
            // Wait until the visual progress actually reaches 100 before hiding
            while (_currentProgress < 100)
            {
                await Task.Delay(50);
            }
            await Task.Delay(200); // Brief final pause
            _progressTimer.Stop();
        }

        private void UpdateProgress(double value, string message)
        {
            _targetProgress = value;
            _currentMessage = message;
        }
    }
}