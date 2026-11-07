using DiNet.CliToolkit.Core.Attributes;
using DiNet.CliToolkit.Core.Exceptions;
using DiNet.CliToolkit.Core.Intrerfaces;
using System.Reflection;

namespace DiNet.CliToolkit.Core.Descriptors;

public interface IConstructorInvoker
{
    object Invoke(IConstructorDescriptor descriptor, Func<Type, object> createComplex);
    object InvokeRoot(IRootConstructorDescriptor descriptor, Func<Type, object> createComplex);
}

public class ConstructorInvoker(
    IParameterDeserializer deserializer,
    IFlatArguments arguments,
    IComplexTypeClassifier classifier) : IConstructorInvoker
{
    public object Invoke(IConstructorDescriptor descriptor, Func<Type, object> createComplex)
        => descriptor.Constructor.Invoke(
            GetConstructorArguments(descriptor, createComplex).ToArray());

    public object InvokeRoot(IRootConstructorDescriptor descriptor, Func<Type, object> createComplex)
        => descriptor.Constructor.Invoke(
            [.. GetConstructorArguments(descriptor, createComplex, descriptor.UnflaggedParameter)]);


    private IEnumerable<object?> GetConstructorArguments(
        IConstructorDescriptor descriptor,
        Func<Type, object> createComplex,
        ParameterInfo? firstUnflagged = null)
    {
        if (firstUnflagged is { } param)
        {
            yield return GetParameter(
                arguments.UnflaggedArgument ?? arguments.GetArgument(param.Name!),
                param,
                createComplex);
        }

        foreach (var parameter in descriptor.Parameters)
        {
            var argument = arguments.GetArgument(parameter.Name!);
            var parameterInstance = GetParameter(argument, parameter, createComplex);

            yield return parameterInstance;
        }
    }

    private object GetParameter(ParameterSource? arg, ParameterInfo info, Func<Type, object> createComplex)
    {
        bool isPropertyEndValue = info.IsDefined(typeof(LeafAttribute), inherit: false);

        if (arg is not null)
        {
            var value = deserializer.Deserialize(arg, info.ParameterType);
            if (value is not null)
                return value;
        }

        if (!isPropertyEndValue && classifier.IsComplex(info.ParameterType))
        {
            try
            {
                return createComplex(info.ParameterType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to create an instance of '{info.ParameterType.Name}' " +
                    $"for parameter '{info.Name}'.",
                    ex);
            }
        }

        if (arg is null)
        {
            if (!info.IsOptional)
                throw new ArgumentException(
                    $"Required argument '{info.Name}' is not provided.", nameof(arg));

            return info.DefaultValue;
        }


        throw new InvalidParameterProvidedException(info.Name);
    }
}