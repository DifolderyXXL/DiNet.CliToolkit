namespace DiNet.CliToolkit.Core.Intrerfaces;

public interface IValueConverter
{
    bool CanConvert(Type targetType);
    bool TryConvert(string value, Type targetType, out object? result);
}
