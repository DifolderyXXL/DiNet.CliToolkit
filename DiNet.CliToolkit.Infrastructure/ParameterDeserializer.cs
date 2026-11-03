using DiNet.CliToolkit.Core.Exceptions;
using DiNet.CliToolkit.Core.Intrerfaces;

namespace DiNet.CliToolkit.Infrastructure;


public class ParameterDeserializer(IReadOnlyList<IValueConverter> converters) : IParameterDeserializer
{
    public object? Deserialize(ParameterSource source, Type targetType)
    {
        if (source.Name != null && source.Value == null) // flag (--flag)
        {
            if (targetType != typeof(bool))
                throw new FlagsCanOnlyBeAppliedToBooleanException();

            return true;
        }

        if (source.Value == null)
        {
            // For nullable Null
            if (IsNullableType(targetType))
                return null;

            throw new ArgumentException($"Value is null but target type '{targetType.Name}' does not allow null.");
        }

        foreach (var converter in converters)
        {
            if (!converter.CanConvert(targetType)) continue;

            if (converter.TryConvert(source.Value, targetType, out var result))
                return result;

            throw new InvalidOperationException(
                $"Cannot convert value '{source.Value}' to type '{targetType.Name}' " +
                $"using {converter.GetType().Name}.");
        }

        throw new NotSupportedException(
            $"No converter registered for type '{targetType.Name}'.");
    }

    private static bool IsNullableType(Type type)
    {
        return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
    }
}
