namespace DiNet.CliToolkit.Core.Descriptors;

public interface IObjectDescriptorFactory
{
    public IObjectDescriptor Create(Type type);
}

public class ObjectDescriptorFactory : IObjectDescriptorFactory
{
    public IObjectDescriptor Create(Type type)
        => new ObjectDescriptor(type);
}


public interface IObjectAssembler
{
    object CreateRoot(Type type);
    object Create(Type type);
}

public class ObjectAssembler(
    IDescriptorContext context,
    IComplexTypeClassifier classifier,
    IConstructorInvoker constructorInvoker,
    IPropertyInjector injector) : IObjectAssembler
{
    public object CreateRoot(Type type)
    {
        var rawDescriptor = context.GetOrGenerateObjectDescriptor(type);
        var rootDescriptor = new RootObjectDescriptor(rawDescriptor, classifier);

        var instance = constructorInvoker.InvokeRoot(rootDescriptor.SingleConstructor, Create);
        injector.Inject(instance, rootDescriptor, Create);
        return instance;
    }

    public object Create(Type type)
    {
        var descriptor = context.GetOrGenerateObjectDescriptor(type);
        var instance = constructorInvoker.Invoke(descriptor.SingleConstructor, Create);
        injector.Inject(instance, descriptor, Create);
        return instance;
    }
}