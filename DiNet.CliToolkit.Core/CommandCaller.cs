using DiNet.CliToolkit.Core.Attributes;
using DiNet.CliToolkit.Core.Exceptions;
using DiNet.CliToolkit.Core.Intrerfaces;
using System.Data;
using System.Reflection;

namespace DiNet.CliToolkit.Core;

public class CommandCaller
{
    public record PrenamedParameterInfo(string FormattedName, ParameterInfo Parameter);

    public readonly ConstructorInfo Constructor;
    public readonly ParameterInfo? FirstUnflaggedParameter;
    public readonly PrenamedParameterInfo[] Parameters;
    public readonly Type Type;

    private CommandCaller(
        ConstructorInfo constructor,
        ParameterInfo? firstUnflaggedParameter,
        PrenamedParameterInfo[] parameters)
    {
        Constructor = constructor;
        FirstUnflaggedParameter = firstUnflaggedParameter;
        Parameters = parameters;
        Type = constructor.DeclaringType!;
    }

    /// <summary>
    /// Static factory method to generate valid caller metadata
    /// </summary>
    /// <typeparam name="T">Command type</typeparam>
    /// <param name="formatter">Formatter converts names of command constructor parameters</param>
    /// <returns>Caller metadata</returns>
    /// <exception cref="IncorrectCommandDefenitionException"></exception>
    public static CommandCaller CreateFor(Type type, INameFormatter formatter)
    {
        ConstructorInfo constructor = type.GetConstructors()
            .Single();

        ParameterInfo[] parameters = constructor.GetParameters();

        ParameterInfo? firstUnflagged = null;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].GetCustomAttribute<NotFlaggedAttribute>() != null)
            {
                firstUnflagged = parameters[i]; // only first will be cought, else exeption prevent execution

                if (i != 0)
                    throw new IncorrectCommandDefenitionException();
            }
        }

        var extraParameters = firstUnflagged == null
            ? parameters
            : parameters.Skip(1);

        var transformedParameters = extraParameters
            .Select(x => new PrenamedParameterInfo(formatter.Format(x.Name!), x))
            .ToArray();

        // assert if any duplicated names appeared
        var duplicates = transformedParameters
           .GroupBy(info => info.FormattedName)
           .Where(g => g.Count() > 1)
           .Select(g => g.Key)
           .ToArray();

        if (duplicates.Length > 0)
            throw new IncorrectCommandDefenitionException("Duplicated names after formatting");

        return new CommandCaller(constructor, firstUnflagged, transformedParameters);
    }

    public static CommandCaller CreateFor<T>(INameFormatter formatter)
    {
        return CreateFor(typeof(T), formatter);
    }
}

public interface ICommandTypeClassifier
{
    bool IsCommand(Type type);
}


public sealed record ParameterMetadata(
    string Name,
    Type ParameterType,
    CommandMetadata? NestedCommand);

public sealed record PropertyMetadata(
    string Name,
    Type PropertyType,
    TargetMetadata? Target,
    CommandMetadata? NestedCommand);

public sealed record TargetMetadata(
    string FormattedName,
    Type TargetType,
    bool IsOptional,
    object? DefaultValue);

public sealed record CommandMetadata(
    Type CommandType,
    TargetMetadata? Target,
    IReadOnlyList<ParameterMetadata> Parameters,
    IReadOnlyList<PropertyMetadata> Properties);

public interface ICommandMetadataFactory
{
    CommandMetadata Create(Type commandType);
}

public sealed class CommandMetadataFactory : ICommandMetadataFactory
{
    private readonly INameFormatter _formatter;
    private readonly ICommandTypeClassifier _classifier;

    public CommandMetadataFactory(INameFormatter formatter, ICommandTypeClassifier classifier)
    {
        _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        _classifier = classifier ?? throw new ArgumentNullException(nameof(classifier));
    }

    public CommandMetadata Create(Type commandType)
        => Create(commandType, new HashSet<Type>(), true);


    private CommandMetadata Create(Type commandType, HashSet<Type> path, bool canAssignUnflagged)
    {
        if (commandType is null) throw new ArgumentNullException(nameof(commandType));

        if (!path.Add(commandType))
            throw new IncorrectCommandDefenitionException(
                $"Cyclic command reference detected for '{commandType.FullName}'.");

        try
        {
            //
            var constructor = SelectSingleConstructor(commandType);
            var rawParameters = constructor.GetParameters();

            TargetMetadata? constructorUnflagged = GetTarget(rawParameters.FirstOrDefault());

            if (constructorUnflagged != null && _classifier.IsCommand(constructorUnflagged.TargetType))
            {
                throw new IncorrectCommandDefenitionException(
                    $"Nested object should not be primitive.");
            }

            var enumerableParameters = constructorUnflagged == null
                ? rawParameters
                : rawParameters.Skip(1);

            var flagged = enumerableParameters.Select(p => ToMetadata(p, path)).ToArray();
            //

            var properties = GetProperties(commandType, path);

            var propertyUnflagged = GetSingleUnflagged(properties);
            var propertiesQuery = properties.Where(x=>x.Target != propertyUnflagged);

            if (!canAssignUnflagged && (constructorUnflagged != null || propertyUnflagged != null))
            {
                throw new IncorrectCommandDefenitionException(
                    $"Unflagged can't be deep nested.");
            }

            if (constructorUnflagged != null && propertyUnflagged != null)
            {
                throw new IncorrectCommandDefenitionException(
                    $"Can't provide more than one unflagged arguments.");
            }

            var anyUnflagged = constructorUnflagged ?? propertyUnflagged;

            return new(commandType, anyUnflagged, flagged, [.. propertiesQuery]);
        }
        finally
        {
            path.Remove(commandType);
        }
    }

    public TargetMetadata? GetSingleUnflagged(IReadOnlyList<PropertyMetadata> properties)
    {
        var target = properties.SingleOrDefault(x => x.Target != null);
        return target?.Target;
    }

    public TargetMetadata? GetTarget(PropertyInfo? property)
    {
        var flag = property?.GetCustomAttribute<NotFlaggedAttribute>();

        return flag == null
            ? null
            : TargetFromProperty(property!);
    }

    public TargetMetadata TargetFromProperty(PropertyInfo property)
    {
        var name = property.Name
            ?? throw new IncorrectCommandDefenitionException(
                $"Parameter of '{property.DeclaringType?.FullName}' has no name.");

        return new(_formatter.Format(name), property.PropertyType, true, null);
        // true as optional, and null as default value that won't be applied
    }

    public TargetMetadata? GetTarget(ParameterInfo? parameter)
    {
        var flag = parameter?.GetCustomAttribute<NotFlaggedAttribute>();

        return flag == null
            ? null
            : TargetFromParameter(parameter!);
    }

    public TargetMetadata TargetFromParameter(ParameterInfo parameter)
    {
        var name = parameter.Name
            ?? throw new IncorrectCommandDefenitionException(
                $"Parameter of '{parameter.Member.DeclaringType?.FullName}' has no name.");

        return new(_formatter.Format(name), parameter.ParameterType, parameter.IsOptional, parameter.DefaultValue);
    }

    private ParameterMetadata ToMetadata(ParameterInfo parameter, HashSet<Type> path)
    {
        var name = parameter.Name
            ?? throw new IncorrectCommandDefenitionException(
                $"Parameter of '{parameter.Member.DeclaringType?.FullName}' has no name.");

        var nested = _classifier.IsCommand(parameter.ParameterType)
            ? Create(parameter.ParameterType, path, false)
            : null;

        return new ParameterMetadata(_formatter.Format(name), parameter.ParameterType, nested);
    }

    private IReadOnlyList<PropertyMetadata> GetProperties(Type type, HashSet<Type> path)
    {
        if (type is null) throw new ArgumentNullException(nameof(type));

        if (!path.Add(type))
            throw new IncorrectCommandDefenitionException(
                $"Cyclic property reference detected for '{type.FullName}'.");

        try
        {
            return type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetIndexParameters().Length == 0)
                .Select(p => new PropertyMetadata(
                    _formatter.Format(p.Name),
                    p.PropertyType,
                    GetTarget(p),
                    _classifier.IsCommand(p.PropertyType)
                        ? Create(p.PropertyType, path, false)
                        : null))
                .ToArray();
        }
        finally
        {
            path.Remove(type);
        }
    }

    private ConstructorInfo SelectSingleConstructor(Type commandType)
    {
        var constructors = commandType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        return constructors.Length switch
        {
            1 => constructors[0],
            0 => throw new IncorrectCommandDefenitionException(
                     $"Command '{commandType.FullName}' has no public constructor."),
            _ => throw new IncorrectCommandDefenitionException(
                     $"Command '{commandType.FullName}' must declare exactly one public constructor.")
        };
    }
}
/*

public interface IDescriptorContext
{
    public IObjectDescriptor GetObjectDescriptor(Type type);
}


public interface IConstructorDescriptor
{
    public ConstructorInfo Constructor { get; }
    public IEnumerable<ParameterInfo> Parameters { get; }
}


public interface IObjectDescriptor
{
    public Type Type { get;}
    public IConstructorDescriptor SingleConstructor { get; }
    public IEnumerable<PropertyInfo> Properties { get; }
}
public interface IRootConstructorDescriptor : IConstructorDescriptor
{
    public ParameterInfo? UnflaggedParameter { get; }
}
public interface IRootObjectDescriptor : IObjectDescriptor
{
    public PropertyInfo? UnflaggedProperty { get; }
}

public class ObjectDescriptor(Type type) : IObjectDescriptor
{
    public Type Type => type;

    private readonly Lazy<ConstructorDescriptor> _constructor = new(
        () => new ConstructorDescriptor(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                    .Single())
    );

    public IEnumerable<PropertyInfo> Properties => type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetIndexParameters().Length == 0);

    public IConstructorDescriptor SingleConstructor => _constructor.Value;
}

public class ConstructorDescriptor(ConstructorInfo constructorInfo) : IConstructorDescriptor
{
    public IEnumerable<ParameterInfo> Parameters => constructorInfo.GetParameters();

    public ConstructorInfo Constructor => constructorInfo;
}

public class RootObjectDescriptor(ObjectDescriptor descriptor) : IRootObjectDescriptor
{
    public Type Type => descriptor.Type;

    public IConstructorDescriptor SingleConstructor => descriptor.SingleConstructor;

    public IEnumerable<PropertyInfo> Properties => throw new NotImplementedException();

    public PropertyInfo? UnflaggedProperty => throw new NotImplementedException();
}

public class RootConstructorDescriptor(IConstructorDescriptor descriptor) : IRootConstructorDescriptor
{
    public IEnumerable<ParameterInfo> Parameters => descriptor.Parameters;

    public ConstructorInfo Constructor => descriptor.Constructor;

    public ParameterInfo? UnflaggedParameter
    {
        get
        {
            var parameter = descriptor.Parameters.FirstOrDefault();

            
        }
    }
}*/

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

