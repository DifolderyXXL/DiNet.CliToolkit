using System.Reflection;

namespace DiNet.CliToolkit.Core.Descriptors;

public interface IRootConstructorDescriptor : IConstructorDescriptor
{
    public ParameterInfo? UnflaggedParameter { get; }
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

