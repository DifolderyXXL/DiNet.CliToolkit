using DiNet.CliToolkit.Abstraction;
using DiNet.CliToolkit.Core.Descriptors;
using DiNet.CliToolkit.Core.Intrerfaces;
using DiNet.CliToolkit.Services.Arguments;
using Microsoft.Extensions.DependencyInjection;

namespace DiNet.CliToolkit.Services;

public class CliApplication(
    ICommandCallRequestBuilder commandCallRequestBuilder,
    IQueryHandlerResolver queryHandlerResolver,
    IPresenterResolver presenterResolver,
    IServiceScopeFactory scopeFactory,
    CommandRouteRegistry routeRegistry)
{
    public async Task Route(string[] args, CancellationToken ct)
    {
        if (args.Length == 0) return;

        var route = args[0];
        if (!routeRegistry.TryGetHandlerType(route, out var type) || type is null) return;

        var handler = queryHandlerResolver.Resolve(type);


        var queryArgs = args.Length > 1 ? args.AsSpan(1) : ReadOnlySpan<string>.Empty;
        var flatArgs = commandCallRequestBuilder.Build(queryArgs);


        using var scope = scopeFactory.CreateScope();
        scope.ServiceProvider.GetRequiredService<IFlatArgumentsStore>().Set(flatArgs);


        var assembler = scope.ServiceProvider.GetRequiredService<IObjectAssembler>();
        var query = assembler.CreateRoot(handler.QueryType);


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
