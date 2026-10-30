using DiNet.CliToolkit.Core.Exceptions;
using DiNet.CliToolkit.Core.Intrerfaces;
using System.Reflection;

namespace DiNet.CliToolkit.Core.Descriptors;

public interface IObjectDescriptorFactory
{
    public IObjectDescriptor Create(Type type);
}

public interface IFlatArguments
{
    public ParameterSource? UnflaggedArgument { get; }
    public ParameterSource? GetArgument(string rawName);
}

public class CommandDeserializer(
    IDescriptorContext context, 
    IComplexTypeClassifier classifier, 
    IFlatArguments arguments,
    IParameterDeserializer deserializer)
{
    public object CreateRoot(Type type)
    {
        var rawDescriptor = context.GetOrGenerateObjectDescriptor(type);
        var objectDescriptor = new RootObjectDescriptor(
            rawDescriptor,
            classifier);

        var instance = CreateFromRootConstructor(objectDescriptor.SingleConstructor);
        FillProperties(instance, rawDescriptor);
        return instance;
    }

    private object Create(Type type)
    {
        var objectDescriptor = context.GetOrGenerateObjectDescriptor(type);
        var instance = CreateFromConstructor(objectDescriptor.SingleConstructor);
        FillProperties(instance, objectDescriptor);
        return instance;
    }

    public void FillProperties(object instance, IObjectDescriptor descriptor)
    {
        if (descriptor is IRootObjectDescriptor { UnflaggedProperty: { } unflagged })
        {
            SetProperty(instance, unflagged, arguments.UnflaggedArgument);
        }

        foreach (var property in descriptor.Properties)
        {
            SetProperty(instance, property, arguments.GetArgument(property.Name));
        }
    }

    private void SetProperty(object instance, PropertyInfo property, ParameterSource? arg)
    {
        if (arg is null)
            return;

        if (!property.CanWrite)
            throw new InvalidOperationException(
                $"Property '{property.Name}' on '{property.DeclaringType?.Name}' is not writable.");

        property.SetValue(instance, GetPropertyValue(arg, property));
    }

    private object GetPropertyValue(ParameterSource arg, PropertyInfo property)
    {
        var argValue = deserializer.Deserialize(arg, property.PropertyType);
        if (argValue is not null)
            return argValue;

        var (createdInstance, instanceException) = TryCreateComplexInstance(property.PropertyType);
        if (createdInstance is not null)
            return createdInstance;

        if (instanceException is not null)
            throw new InvalidOperationException(
                $"Failed to create an instance of '{property.PropertyType.Name}' " +
                $"for property '{property.Name}'.",
                instanceException);

        throw new InvalidParameterProvidedException(property.Name);
    }

    private IEnumerable<object?> GetConstructorArguments(
        IConstructorDescriptor descriptor,
        ParameterInfo? firstUnflagged = null)
    {
        if (firstUnflagged is { } param)
        {
            yield return GetParameter(
                arguments.UnflaggedArgument ?? arguments.GetArgument(param.Name!),
                param);
        }

        foreach (var parameter in descriptor.Parameters)
        {
            yield return GetParameter(arguments.GetArgument(parameter.Name!), parameter);
        }
    }

    public object CreateFromConstructor(IConstructorDescriptor descriptor)
        => descriptor.Constructor.Invoke(
            GetConstructorArguments(descriptor).ToArray());

    public object CreateFromRootConstructor(IRootConstructorDescriptor descriptor)
        => descriptor.Constructor.Invoke(
            GetConstructorArguments(descriptor, descriptor.UnflaggedParameter).ToArray());

    private object GetParameter(ParameterSource? arg, ParameterInfo info)
    {
        var (createdInstance, instanceException) = TryCreateComplexInstance(info.ParameterType);

        if (arg is null && !info.IsOptional)
            throw new ArgumentException(
                $"Required argument '{info.Name}' is not provided.", nameof(arg));

        if (arg is null)
            return info.DefaultValue!;

        var argValue = deserializer.Deserialize(arg, info.ParameterType);
        if (argValue is not null)
            return argValue;

        if (createdInstance is not null)
            return createdInstance;

        if (instanceException is not null)
            throw new InvalidOperationException(
                $"Failed to create an instance of '{info.ParameterType.Name}' " +
                $"for parameter '{info.Name}'.",
                instanceException);

        throw new InvalidParameterProvidedException(info.Name);
    }

    private (object? Instance, Exception? Error) TryCreateComplexInstance(Type type)
    {
        if (!classifier.IsComplex(type))
            return (null, null);

        try
        {
            return (Create(type), null);
        }
        catch (Exception ex)
        {
            return (null, ex);
        }
    }

}




/*
 
FlatQuery
- Unflagged parameter
- [name, val]


IContext
    => FlatQuery

InitializeObject
    InitializeConstructor
    InitialierProperties
    => loop
 
// metadataContext

IDescriptorContext
    => ...

ObjDescriptor
    Constructor
    Properties
    => loop

RootObjectDescriptor : ObjDescriptor
    FirstUnflagged


* => O(SEGMENT) => *



 */

