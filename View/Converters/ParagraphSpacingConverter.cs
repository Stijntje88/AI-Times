using Microsoft.UI.Xaml.Data;
using System;

namespace AI_Times.View.Converters
{
    public class ParagraphSpacingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string text)
            {
                // Replace single line breaks with double line breaks for better readability
                // But avoid quadrupling them if they are already double
                text = text.Replace("\r\n", "\n"); // Normalize
                text = text.Replace("\n\n", "\n"); // Remove existing doubles
                return text.Replace("\n", "\n\n"); // Add spacing
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
