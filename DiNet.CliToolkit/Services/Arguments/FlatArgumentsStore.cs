using DiNet.CliToolkit.Core.Descriptors;
using DiNet.CliToolkit.Core.Intrerfaces;

namespace DiNet.CliToolkit.Services.Arguments;

public sealed class FlatArgumentsStore : IFlatArguments, IFlatArgumentsStore
{
    private IFlatArguments? _inner;

    public void Set(IFlatArguments arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        if (_inner is not null)
            throw new InvalidOperationException(
                "IFlatArguments is already set for this scope.");

        _inner = arguments;
    }

    public ParameterSource? UnflaggedArgument => Inner.UnflaggedArgument;
    public ParameterSource? GetArgument(string rawName) => Inner.GetArgument(rawName);

    private IFlatArguments Inner =>
        _inner ?? throw new InvalidOperationException(
            "IFlatArguments is not initialized for this scope. " +
            "Call IFlatArgumentsStore.Set(...) before resolving the query.");
}
