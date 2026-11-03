using DiNet.CliToolkit.Core.Descriptors;

namespace DiNet.CliToolkit.Core.Intrerfaces;

public interface ICommandCallRequestBuilder
{
    public IFlatArguments Build(ReadOnlySpan<string> args);
}
