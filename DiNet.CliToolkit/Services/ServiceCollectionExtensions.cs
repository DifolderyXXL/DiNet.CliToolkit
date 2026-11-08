using DiNet.CliToolkit.Abstraction;
using DiNet.CliToolkit.Core;
using DiNet.CliToolkit.Core.Descriptors;
using DiNet.CliToolkit.Core.Intrerfaces;
using DiNet.CliToolkit.Infrastructure;
using DiNet.CliToolkit.Infrastructure.Formatters;
using DiNet.CliToolkit.Infrastructure.TypeConverters;
using DiNet.CliToolkit.Services.Arguments;
using DiNet.CliToolkit.Services.HelpHandler;
using Microsoft.Extensions.DependencyInjection;

namespace DiNet.CliToolkit.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCliToolkit(
        this IServiceCollection services,
        Action<CliToolkitBuilder> configure)
    {
        var builder = new CliToolkitBuilder(services);
        configure(builder);
        builder.Build();
        return services;
    }
}

public class CliToolkitBuilder
{
    private readonly IServiceCollection _services;
    private readonly CommandRouteRegistry _routeRegistry;
    private readonly PresenterStore _presenterStore;
    private Type? _formatterType;

    public CliToolkitBuilder(IServiceCollection services)
    {
        _services = services;
        _routeRegistry = new CommandRouteRegistry();
        _presenterStore = new PresenterStore();
    }

    public HandlerRegistry Handlers => new(_routeRegistry, _services);

    public PresenterRegistry Presenters => new(_presenterStore, _services);

    public CliToolkitBuilder UseKebabCaseFormatter()
    {
        _formatterType = typeof(KebabCaseNameFormatter);
        return this;
    }

    public CliToolkitBuilder UseCamelCaseFormatter()
    {
        _formatterType = typeof(CamelCaseNameFormatter);
        return this;
    }

    public CliToolkitBuilder UseCaseFormatter<T>() where T : class, INameFormatter
    {
        _formatterType = typeof(T);
        return this;
    }

    public CliToolkitBuilder UseFormatter<TFormatter>()
        where TFormatter : class, INameFormatter
    {
        _formatterType = typeof(TFormatter);
        return this;
    }

    internal void Build()
    {
        _services.AddSingleton(_routeRegistry);
        _services.AddSingleton<IPresenterStore>(_presenterStore);

        var formatterType = _formatterType ?? typeof(DefaultNameFormatter);
        _services.AddSingleton(typeof(INameFormatter), formatterType);

        _services.AddScoped<IParameterDeserializer, ParameterDeserializer>();
        _services.AddScoped<ICommandCallRequestBuilder, StrictCommandCallRequestBuilder>();

        _services.AddTransient<IQueryHandlerResolver, DefaultQueryHandlerResolver>();
        _services.AddTransient<IPresenterResolver, PresenterResolver>();

        _services.AddScoped<IRootDescriptorResolver, RootDescriptorResolver>();

        _services.AddScoped<IObjectDescriptorFactory, ObjectDescriptorFactory>();
        _services.AddScoped<IObjectAssembler, ObjectAssembler>();
        _services.AddScoped<IConstructorInvoker, ConstructorInvoker>();
        _services.AddScoped<IDescriptorContext, DescriptorContext>();
        _services.AddScoped<IPropertyInjector, PropertyInjector>();
        _services.AddScoped<IComplexTypeClassifier, CommandTypeClassifier>();


        _services.AddSingleton<IValueConverter, TypeConverterAdapter>();
        _services.AddSingleton<IValueConverter, ParsableTypeConverterAdapter>();


        AddArguments();


        _services.AddTransient<CliApplication>();
    }

    private void AddArguments()
    {
        _services.AddScoped<IFlatArgumentsFactory, FlatArgumentsFactory>();
        _services.AddScoped<FlatArgumentsStore>();
        _services.AddScoped<IFlatArguments>(sp => sp.GetRequiredService<FlatArgumentsStore>());
        _services.AddScoped<IFlatArgumentsStore>(sp => sp.GetRequiredService<FlatArgumentsStore>());
    }
}

public class HandlerRegistry
{
    private readonly CommandRouteRegistry _registry;
    private readonly IServiceCollection _services;

    public HandlerRegistry(CommandRouteRegistry registry, IServiceCollection services)
    {
        _registry = registry;
        _services = services;
    }

    public HandlerRegistry Register<THandler>(string route) where THandler : class
    {
        _registry.Register<THandler>(route);
        _services.AddTransient<THandler>();
        return this;
    }

    public HandlerRegistry IntroduceHelpCommand(string route)
    {
        _registry.Register<HelpCommandHandler>(route);
        _services.AddTransient<HelpCommandHandler>();
        return this;
    }
}


public class PresenterRegistry
{
    private readonly PresenterStore _store;
    private readonly IServiceCollection _services;

    public PresenterRegistry(PresenterStore store, IServiceCollection services)
    {
        _store = store;
        _services = services;
    }

    public PresenterRegistry Register<TResponse, TPresenter>()
        where TPresenter : class, IDataPresenter<TResponse>
    {
        _store.Register<TResponse, TPresenter>();
        _services.AddTransient<TPresenter>();
        return this;
    }
}
