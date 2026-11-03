namespace DiNet.CliToolkit.Core;

public class CommandTypeClassifier : IComplexTypeClassifier
{
    public bool IsComplex(Type type)
    {
        return type is { IsClass: true, IsAbstract: false } && !typeof(string).IsEquivalentTo(type);
    }
}