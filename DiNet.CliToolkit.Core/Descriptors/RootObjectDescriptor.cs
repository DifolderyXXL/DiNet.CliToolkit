using DiNet.CliToolkit.Core.Attributes;
using DiNet.CliToolkit.Core.Exceptions;
using System.Data;
using System.Reflection;

namespace DiNet.CliToolkit.Core.Descriptors;

public class RootObjectDescriptor : IRootObjectDescriptor
{
    public Type Type { get; }

    public IRootConstructorDescriptor SingleConstructor { get; }

    public IEnumerable<PropertyInfo> Properties { get; }

    public PropertyInfo? UnflaggedProperty { get; }


    public RootObjectDescriptor(IObjectDescriptor descriptor, IComplexTypeClassifier classifier)
    {
        Type = descriptor.Type;

        SingleConstructor = new RootConstructorDescriptor(descriptor.SingleConstructor, classifier);

        var parameters = descriptor.Properties.ToArray();

        var notFlagged = parameters
            .SingleOrDefault(x => x?.GetCustomAttribute<NotFlaggedAttribute>() != null);

        if (notFlagged != null && classifier.IsComplex(notFlagged.PropertyType))
            throw new IncorrectCommandDefenitionException($"NotFlagged can't be applied to complex types  [{notFlagged.PropertyType.FullName}].");

        UnflaggedProperty = notFlagged;

        if (UnflaggedProperty != null && SingleConstructor.UnflaggedParameter != null)
            throw new IncorrectCommandDefenitionException("NotFlagged can't be applied to constructor and properties.");

        Properties = [.. parameters.Where(x => x != notFlagged)];
    }
}