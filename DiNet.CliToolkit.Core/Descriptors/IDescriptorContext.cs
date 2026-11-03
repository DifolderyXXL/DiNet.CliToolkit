namespace DiNet.CliToolkit.Core.Descriptors;

public interface IDescriptorContext
{
    public IObjectDescriptor GetOrGenerateObjectDescriptor(Type type);
}