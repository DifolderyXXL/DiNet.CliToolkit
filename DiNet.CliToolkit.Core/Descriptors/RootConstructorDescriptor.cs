using DiNet.CliToolkit.Core.Attributes;
using DiNet.CliToolkit.Core.Exceptions;
using System.Reflection;

namespace DiNet.CliToolkit.Core.Descriptors;

public class RootConstructorDescriptor : IRootConstructorDescriptor
{
    public IEnumerable<ParameterInfo> Parameters => _parameters;

    public ConstructorInfo Constructor { get; }

    public ParameterInfo? UnflaggedParameter { get; }

    private ParameterInfo[] _parameters;

    public RootConstructorDescriptor(IConstructorDescriptor descriptor, IComplexTypeClassifier classifier)
    {
        var firstNotFlagged = descriptor.Parameters
            .SingleOrDefault(x => x?.GetCustomAttribute<NotFlaggedAttribute>() != null);

        if (firstNotFlagged != null)
        {
            if (firstNotFlagged.Position != 0)
                throw new IncorrectCommandDefenitionException("NotFlagged can be applied to the first constructor parameter.");

            if (classifier.IsComplex(firstNotFlagged.ParameterType))
                throw new IncorrectCommandDefenitionException($"NotFlagged can't be applied to complex types  [{firstNotFlagged.ParameterType.FullName}].");
        }

        UnflaggedParameter = firstNotFlagged;

        var parametersEnumerable = UnflaggedParameter == null
            ? descriptor.Parameters
            : descriptor.Parameters.Skip(1);

        _parameters = parametersEnumerable.ToArray();

        Constructor = descriptor.Constructor;
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

