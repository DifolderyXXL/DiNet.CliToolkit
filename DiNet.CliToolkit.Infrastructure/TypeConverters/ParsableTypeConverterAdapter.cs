using DiNet.CliToolkit.Core.Intrerfaces;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace DiNet.CliToolkit.Infrastructure.TypeConverters;

public sealed class ParsableTypeConverterAdapter : IValueConverter
{
    private delegate bool TryParseFn(string? s, IFormatProvider? p, out object? result);

    private static readonly ConcurrentDictionary<Type, TryParseFn?> _cache = new();

    public bool CanConvert(Type targetType) => _cache.GetOrAdd(targetType, Build) is not null;

    public bool TryConvert(string value, Type targetType, out object? result)
    {
        var fn = _cache.GetOrAdd(targetType, Build);
        if (fn is null)
        {
            result = null;
            return false;
        }
        return fn(value, CultureInfo.InvariantCulture, out result);
    }

    private static TryParseFn? Build(Type targetType)
    {
        Type parsable;
        try { parsable = typeof(IParsable<>).MakeGenericType(targetType); }
        catch (ArgumentException) { return null; }
        if (!parsable.IsAssignableFrom(targetType)) return null;

        return typeof(ParsableTypeConverterAdapter)
            .GetMethod(nameof(TryParseImpl), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(targetType)
            .CreateDelegate<TryParseFn>();
    }

    private static bool TryParseImpl<T>(string? s, IFormatProvider? p, out object? result)
        where T : IParsable<T>
    {
        if (T.TryParse(s, p, out var v))
        {
            result = v;
            return true;
        }
        result = null;
        return false;
    }
}