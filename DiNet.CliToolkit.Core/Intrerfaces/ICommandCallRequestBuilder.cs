using DiNet.CliToolkit.Core.Models;

namespace DiNet.CliToolkit.Core.Intrerfaces;

public interface ICommandCallRequestBuilder
{
    public CommandCallRequest Build(ReadOnlySpan<string> args);
}
