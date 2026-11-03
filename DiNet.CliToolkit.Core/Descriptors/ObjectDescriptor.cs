using System.Data;
using System.Reflection;

namespace DiNet.CliToolkit.Core.Descriptors;

public class ObjectDescriptor : IObjectDescriptor
{
    public Type Type { get; }

    public IEnumerable<PropertyInfo> Properties { get; }

    public IConstructorDescriptor SingleConstructor { get; }

    public ObjectDescriptor(Type type)
    {
        Type = type;
        SingleConstructor = new ConstructorDescriptor(
            type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                    .Single());

        Properties = [.. type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetIndexParameters().Length == 0)
            .Where(p => p.CanWrite)];

    }
}
