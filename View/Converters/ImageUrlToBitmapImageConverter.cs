using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;

namespace AI_Times.View.Converters
{
    public sealed class ImageUrlToBitmapImageConverter : IValueConverter
    {
        private static readonly Uri PlaceholderUri = new("ms-appx:///Assets/AI-Times-Logo-Inside-App.png");

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var url = value as string;

            if (string.IsNullOrWhiteSpace(url))
            {
                return new BitmapImage(PlaceholderUri);
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return new BitmapImage(PlaceholderUri);
            }

            try
            {
                var bmp = new BitmapImage(uri);
                bmp.ImageFailed += (sender, e) =>
                {
                    if (sender is BitmapImage b)
                    {
                        b.UriSource = PlaceholderUri;
                    }
                };
                return bmp;
            }
            catch
            {
                return new BitmapImage(PlaceholderUri);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
