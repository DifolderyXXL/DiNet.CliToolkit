using System.Reflection;

namespace DiNet.CliToolkit.Core.Descriptors;

public interface IObjectDescriptor<out TConstructor>
    where TConstructor : IConstructorDescriptor
{
    Type Type { get; }
    TConstructor SingleConstructor { get; }
    IEnumerable<PropertyInfo> Properties { get; }
}

public interface IObjectDescriptor : IObjectDescriptor<IConstructorDescriptor> { }