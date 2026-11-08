using DiNet.CliToolkit.Abstraction;
using DiNet.CliToolkit.Core;
using DiNet.CliToolkit.Core.Descriptors;
using DiNet.CliToolkit.Core.Intrerfaces;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;

namespace DiNet.CliToolkit.Services.HelpHandler;


public record HelpCommand : IQuery<bool>;
public class HelpCommandHandler(
    CommandRouteRegistry registry,
    IRootDescriptorResolver resolver,
    IObjectDescriptorFactory factory,
    IComplexTypeClassifier classifier,
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

        AppendNestedParameter(sb, descriptor.Type);

        return sb.ToString();
    }

    private void AppendNestedParameter(StringBuilder sb, Type type)
    {
        var descriptor = factory.Create(type);

        foreach (var parameter in descriptor.SingleConstructor.Parameters)
        {
            if(classifier.IsComplex(parameter.ParameterType))
            {
                AppendNestedParameter(sb, parameter.ParameterType);
            }
            else
            {
                sb.Append(Indent)
                  .Append("--").Append(formatter.Format(parameter.Name!)).Append(' ')
                  .AppendLine(FormatParameter(parameter, includeName: false));
            }
        }

        var ctorNames = descriptor.SingleConstructor.Parameters
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var property in descriptor.Properties)
        {
            if (ctorNames.Contains(property.Name)) continue;

            if (classifier.IsComplex(property.PropertyType))
            {
                AppendNestedParameter(sb, property.PropertyType);
            }
            else
            {
                sb.Append(Indent)
                  .Append("--").Append(formatter.Format(property.Name)).Append(' ')
                  .AppendLine(FormatProperty(property));
            }
        }
    }

    private static string FormatParameter(ParameterInfo info, bool includeName)
    {
        var optional = info.IsOptional ? "(opt)" : string.Empty;
        var name = includeName ? $"[{info.Name}]" : string.Empty;
        return $"<{FormatType(info.ParameterType)}>{optional}{name}";
    }

    private static string FormatProperty(PropertyInfo info)
    {
        var isRequired = info.IsDefined(typeof(System.Runtime.CompilerServices.RequiredMemberAttribute), inherit: false);

        var optional = !isRequired ? "(opt)" : string.Empty;
        return $"<{FormatType(info.PropertyType)}>{optional}";
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