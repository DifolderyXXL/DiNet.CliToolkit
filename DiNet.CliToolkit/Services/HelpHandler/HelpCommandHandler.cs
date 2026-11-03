using DiNet.CliToolkit.Abstraction;
using DiNet.CliToolkit.Core;
using DiNet.CliToolkit.Core.Descriptors;
using DiNet.CliToolkit.Core.Intrerfaces;
using System.Reflection;
using System.Text;

namespace DiNet.CliToolkit.Services.HelpHandler;


public record HelpCommand : IQuery<bool>;
public class HelpCommandHandler(
    CommandRouteRegistry registry,
    IRootDescriptorResolver resolver,
    INameFormatter formatter) : IQueryHandler<HelpCommand, bool>
{
    private const string Indent = "  ";
    public async Task<bool> Handle(HelpCommand query, CancellationToken ct)
    {
        foreach (var kvp in registry.Routes)
        {
            var rootDescriptor = resolver.Resolve(kvp.Value);

            Console.WriteLine(BuildHelp(kvp.Key, rootDescriptor));
        }

        return true;
    }

    private string BuildHelp(string prefix, IRootObjectDescriptor descriptor)
    {
        var sb = new StringBuilder();
        sb.AppendLine(prefix);

        var constructor = descriptor.SingleConstructor;

        if (constructor.UnflaggedParameter is { } unflagged)
        {
            sb.Append(Indent).AppendLine(FormatParameter(unflagged, includeName: true));
        }

        foreach (var parameter in constructor.Parameters)
        {
            sb.Append(Indent)
              .Append("--").Append(formatter.Format(parameter.Name!)).Append(' ')
              .AppendLine(FormatParameter(parameter, includeName: false));
        }

        var ctorNames = constructor.Parameters
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var property in descriptor.Properties)
        {
            if (ctorNames.Contains(property.Name)) continue;

            sb.Append(Indent)
              .Append("--").Append(formatter.Format(property.Name)).Append(' ')
              .AppendLine(FormatProperty(property));
        }

        return sb.ToString();
    }

    private static string FormatParameter(ParameterInfo info, bool includeName)
    {
        var optional = info.IsOptional ? "(opt)" : string.Empty;
        var name = includeName ? $"[{info.Name}]" : string.Empty;
        return $"<{FormatType(info.ParameterType)}>{optional}{name}";
    }

    private static string FormatProperty(PropertyInfo info)
    {
        return $"<{FormatType(info.PropertyType)}>";
    }

    private static string FormatType(Type type)
    {
        if (Nullable.GetUnderlyingType(type) is { } underlying)
            return FormatType(underlying) + "?";

        if (type.IsArray)
            return FormatType(type.GetElementType()!) + "[]";

        return type.Name;
    }
}


public interface IRootDescriptorResolver
{
    IRootObjectDescriptor Resolve(Type handlerType);
}

public sealed class RootDescriptorResolver(
    IDescriptorContext descriptorContext,
    IComplexTypeClassifier classifier) : IRootDescriptorResolver
{
    public IRootObjectDescriptor Resolve(Type handlerType)
    {
        var queryType = ResolveQueryType(handlerType);
        var rawDescriptor = descriptorContext.GetOrGenerateObjectDescriptor(queryType);
        return new RootObjectDescriptor(rawDescriptor, classifier);
    }

    private static Type ResolveQueryType(Type handlerType)
    {
        var handlerInterface = handlerType.GetInterfaces()
            .FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))
            ?? throw new InvalidOperationException(
                $"Type {handlerType.FullName} does not implement IQueryHandler<TQuery, TResponse>");

        return handlerInterface.GetGenericArguments()[0];
    }
}