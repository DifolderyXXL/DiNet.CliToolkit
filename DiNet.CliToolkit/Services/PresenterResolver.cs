using Microsoft.Extensions.DependencyInjection;
using DiNet.CliToolkit.Abstraction;

namespace DiNet.CliToolkit.Services;

public class PresenterResolver(IPresenterStore presenterStore, IServiceProvider provider) : IPresenterResolver
{
    public IUntypedDataPresenter? TryResolve(Type responseType)
    {
        var presenterType = presenterStore.GetPresenterTypeFor(responseType);
        if (presenterType is null) return null;

        var rawPresenter = ActivatorUtilities.CreateInstance(provider, presenterType)!;
        var decoratorType = typeof(UntypedDataPresenterDecorator<>).MakeGenericType(responseType);

        return (IUntypedDataPresenter)Activator.CreateInstance(decoratorType, rawPresenter)!;
    }
}
