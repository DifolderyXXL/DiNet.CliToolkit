using System.Reflection;

namespace DiNet.CliToolkit.Core.Descriptors;

public class ConstructorDescriptor(ConstructorInfo constructorInfo) : IConstructorDescriptor
{
    public IEnumerable<ParameterInfo> Parameters => constructorInfo.GetParameters();

    public ConstructorInfo Constructor => constructorInfo;
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

