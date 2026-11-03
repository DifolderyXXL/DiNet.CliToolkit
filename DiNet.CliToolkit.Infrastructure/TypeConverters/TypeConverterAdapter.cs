using DiNet.CliToolkit.Core.Intrerfaces;
using System.ComponentModel;
using System.Globalization;

namespace DiNet.CliToolkit.Infrastructure.TypeConverters;

public sealed class TypeConverterAdapter : IValueConverter
{
    public bool CanConvert(Type targetType)
        => TypeDescriptor.GetConverter(targetType).CanConvertFrom(typeof(string));

    public bool TryConvert(string value, Type targetType, out object? result)
    {
        try
        {
            result = TypeDescriptor.GetConverter(targetType)
                .ConvertFromString(null, CultureInfo.InvariantCulture, value);
            return result is not null;
        }
        catch
        {
            result = null;
            return false;
        }
    }
}
