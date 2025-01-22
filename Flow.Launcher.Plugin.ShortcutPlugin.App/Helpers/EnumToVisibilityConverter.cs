using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Helpers;

public class EnumToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is string enumString && value != null)
        {
            if (!Enum.IsDefined(value.GetType(), value))
            {
                throw new ArgumentException("ExceptionEnumToVisibilityConverterValueMustBeAnEnum");
            }

            var enumValue = Enum.Parse(value.GetType(), enumString);

            return enumValue.Equals(value) ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}