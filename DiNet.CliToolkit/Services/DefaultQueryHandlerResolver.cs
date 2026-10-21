using Microsoft.Extensions.DependencyInjection;
using DiNet.CliToolkit.Abstraction;

namespace DiNet.CliToolkit.Services;

public class DefaultQueryHandlerResolver(IServiceProvider provider) : IQueryHandlerResolver
{
    public IUntypedQueryHandler Resolve(Type handlerType)
    {
        var handlerInterface = handlerType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))
            ?? throw new InvalidOperationException($"Type {handlerType.FullName} does not implement IQueryHandler<TQuery, TResponse>");

        var genericArgs = handlerInterface.GetGenericArguments();
        var queryType = genericArgs[0];
        var responseType = genericArgs[1];

        var rawHandler = ActivatorUtilities.CreateInstance(provider, handlerType)
            ?? throw new InvalidOperationException($"Could not instantiate {handlerType.Name}");

        var decoratorType = typeof(UntypedQueryHandlerDecorator<,>).MakeGenericType(queryType, responseType);

        return (IUntypedQueryHandler)Activator.CreateInstance(decoratorType, rawHandler)!;
    }
}
