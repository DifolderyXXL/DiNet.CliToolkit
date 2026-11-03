namespace DiNet.CliToolkit.Core;

public class CommandTypeClassifier : IComplexTypeClassifier
{
    public bool IsComplex(Type type)
    {
        return type is { IsClass: true, IsAbstract: false } && !typeof(string).IsEquivalentTo(type);
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
 */

