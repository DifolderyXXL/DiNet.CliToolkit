using System.Reflection;

namespace DiNet.CliToolkit.Core.Descriptors;

public interface IRootObjectDescriptor : IObjectDescriptor<IRootConstructorDescriptor>
{
    PropertyInfo? UnflaggedProperty { get; }
}