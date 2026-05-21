using System.Globalization;

namespace ShopApp.Mobile.Helpers;

/// <summary>Returns the first character of a string uppercased — used for avatar initials.</summary>
public class FirstLetterConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string s && s.Length > 0 ? s[0].ToString().ToUpperInvariant() : "?";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}