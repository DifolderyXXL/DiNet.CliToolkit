using DiNet.CliToolkit.Common.Graphs;
using DiNet.CliToolkit.Core.Descriptors;
using System.Data;

namespace DiNet.CliToolkit.Core;

public static class CommandGraphHelper
{
    public static void TraverseFor(Type type, IDescriptorContext context, IComplexTypeClassifier classifier)
    {
        GraphTraverseHelper<IObjectDescriptor, Type>.TraverseAcyclicGraph(type,
            new(
                descriptor => descriptor.Type,
                context.GetOrGenerateObjectDescriptor,
                descriptor =>
                {
                    var props = descriptor.Properties.Select(x => x.PropertyType);
                    var param = descriptor.SingleConstructor.Parameters.Select(x => x.ParameterType);
                    return props.Concat(param);
                },
                (descriptor, children) =>
                {

                },
                type => !classifier.IsComplex(type)
            )
        );
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

