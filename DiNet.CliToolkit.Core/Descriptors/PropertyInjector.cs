using DiNet.CliToolkit.Core.Attributes;
using DiNet.CliToolkit.Core.Exceptions;
using DiNet.CliToolkit.Core.Intrerfaces;
using System.Reflection;

namespace DiNet.CliToolkit.Core.Descriptors;

public interface IPropertyInjector
{
    void Inject(object instance, IObjectDescriptor<IConstructorDescriptor> descriptor, Func<Type, object> createComplex);
}

public class PropertyInjector(
    IParameterDeserializer deserializer,
    IFlatArguments arguments,
    IComplexTypeClassifier classifier) : IPropertyInjector
{
    public void Inject(object instance, IObjectDescriptor<IConstructorDescriptor> descriptor, Func<Type, object> createComplex)
    {
        if (descriptor is IRootObjectDescriptor { UnflaggedProperty: { } unflagged } objDescriptor)
        {
            SetProperty(instance, unflagged,
                arguments.UnflaggedArgument ?? arguments.GetArgument(objDescriptor.UnflaggedProperty.Name),
                createComplex);
        }

        foreach (var property in descriptor.Properties)
        {
            SetProperty(instance, property, arguments.GetArgument(property.Name), createComplex);
        }
    }

    private void SetProperty(object instance, PropertyInfo property, ParameterSource? arg, Func<Type, object> createComplex)
    {
        if (!property.CanWrite)
            throw new InvalidOperationException(
                $"Property '{property.Name}' on '{property.DeclaringType?.Name}' is not writable.");

        bool isPropertyEndValue = property.IsDefined(typeof(LeafAttribute), inherit: false);

        if (arg is not null)
        {
            var value = deserializer.Deserialize(arg, property.PropertyType);
            if (value is not null) { property.SetValue(instance, value); return; }
        }

        if (!isPropertyEndValue && classifier.IsComplex(property.PropertyType))
        {
            property.SetValue(instance, createComplex(property.PropertyType));
        }

        if (property.IsDefined(typeof(System.Runtime.CompilerServices.RequiredMemberAttribute), inherit: false))
            throw new InvalidOperationException(
                $"Required property '{property.Name}' on '{property.DeclaringType?.Name}' " +
                $"was not provided on the command line.");
    }
}