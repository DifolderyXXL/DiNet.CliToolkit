using System.Collections.Concurrent;

namespace DiNet.CliToolkit.Core.Descriptors;

public class DescriptorContext(IObjectDescriptorFactory factory) : IDescriptorContext
{
    private readonly ConcurrentDictionary<Type, IObjectDescriptor> _descriptors = [];

    public IObjectDescriptor GetOrGenerateObjectDescriptor(Type type)
    {
        if (_descriptors.TryGetValue(type, out var descriptor))
        {
            return descriptor;
        }

        descriptor = factory.Create(type);
        if (_descriptors.TryAdd(type, descriptor))
        {
            return descriptor;
        }

        return _descriptors[type];
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


* => O(SEGMENT) => *




 */

