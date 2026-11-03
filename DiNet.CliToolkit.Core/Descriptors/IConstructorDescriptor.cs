using System.Reflection;

namespace DiNet.CliToolkit.Core.Descriptors;

public interface IConstructorDescriptor
{
    public ConstructorInfo Constructor { get; }
    public IEnumerable<ParameterInfo> Parameters { get; }
}
