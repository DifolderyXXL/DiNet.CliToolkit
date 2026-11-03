using DiNet.CliToolkit.Infrastructure.TypeConverters;

namespace DiNet.CliToolkit.Tests;

public class GenericConvertable<T> : IParsable<GenericConvertable<T>>
    where T : IParsable<T>
{
    public T Value { get; }
    public GenericConvertable(T value) => Value = value;

    public static GenericConvertable<T> Parse(string s, IFormatProvider? provider)
        => new(T.Parse(s, provider));

    public static bool TryParse(string? s, IFormatProvider? provider, out GenericConvertable<T>? result)
    {
        if (T.TryParse(s, provider, out var v)) { result = new(v); return true; }
        result = null; return false;
    }

    public override string ToString() => Value?.ToString() ?? string.Empty;
}

public class NotParsable
{
    public int Value { get; set; }
}

public class ParsableTypeConverterAdapterTests
{
    private ParsableTypeConverterAdapter _adapter = new();

    private readonly Type _parsableIntType = typeof(GenericConvertable<int>);
    private readonly Type _notParsableIntType = typeof(NotParsable);

    [Fact]
    public void From_Parsable_Success()
    {
        Assert.True(_adapter.CanConvert(_parsableIntType));

        Assert.True(_adapter.TryConvert("123", _parsableIntType, out var result));

        Assert.Equal(123, ((GenericConvertable<int>)result).Value);
    }

    [Fact]
    public void From_Parsable_With_Bad_Parameter()
    {
        Assert.True(_adapter.CanConvert(_parsableIntType));

        Assert.False(_adapter.TryConvert("HELLO WORLD", _parsableIntType, out var result));

        Assert.Null(result);
    }

    [Fact]
    public void From_NotParsable()
    {
        Assert.False(_adapter.CanConvert(_notParsableIntType));

        Assert.False(_adapter.TryConvert("25", _notParsableIntType, out var result));

        Assert.Null(result);
    }
}