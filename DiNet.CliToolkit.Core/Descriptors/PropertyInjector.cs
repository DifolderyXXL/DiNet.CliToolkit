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
        if (descriptor is IRootObjectDescriptor { UnflaggedProperty: { } unflagged })
        {
            SetProperty(instance, unflagged, arguments.UnflaggedArgument, createComplex);
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


        if (classifier.IsComplex(property.PropertyType))
        {
            if (arg is not null)
            {
                var converted = deserializer.Deserialize(arg, property.PropertyType);
                if (converted is not null)
                {
                    property.SetValue(instance, converted);
                    return;
                }
            }

            try
            {
                property.SetValue(instance, createComplex(property.PropertyType));
            }
            catch (Exception ex) when (!property.PropertyType.IsValueType)
            {
                _ = ex;
                return;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to create an instance of '{property.PropertyType.Name}' " +
                    $"for property '{property.Name}'.",
                    ex);
            }
            return;
        }

        if (arg is null)
            return;

        var value = deserializer.Deserialize(arg, property.PropertyType);
        if (value is not null)
            property.SetValue(instance, value);
    }
}
