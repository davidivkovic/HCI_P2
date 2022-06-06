using System.Windows.Media;

namespace P2.Extensions;

public static class StringExtensions
{
    public static Brush ToBrush(this string hex) => (SolidColorBrush) new BrushConverter().ConvertFrom(hex);
}