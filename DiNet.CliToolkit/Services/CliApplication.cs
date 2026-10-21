using DiNet.CliToolkit.Core.Intrerfaces;
using DiNet.CliToolkit.Abstraction;
using DiNet.CliToolkit.Services;

namespace DiNet.CliToolkit.Services;

public class CliApplication(
    ICommandHandlerFactory resolver,
    ICommandCallRequestBuilder commandCallRequestBuilder,
    IQueryHandlerResolver queryHandlerResolver,
    IPresenterResolver presenterResolver,
    CommandRouteRegistry routeRegistry)
{
    public async Task Route(string[] args, CancellationToken ct)
    {
        if (args.Length == 0) return;

        var route = args[0];
        if (!routeRegistry.TryGetHandlerType(route, out var type) || type is null) return;

        var handler = queryHandlerResolver.Resolve(type);

        var queryArgs = args.Length > 1 ? args.AsSpan(1) : ReadOnlySpan<string>.Empty;
        var query = resolver.Create(handler.QueryType, commandCallRequestBuilder.Build(queryArgs));

        var response = await handler.HandleAsync(query, ct);

        if (response is not null)
        {
            var presenter = presenterResolver.TryResolve(response.GetType());
            if (presenter is not null)
            {
                presenter.Present(response);
            }
        }
    }
}
