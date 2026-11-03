using DiNet.CliToolkit.Core.Intrerfaces;

namespace DiNet.CliToolkit.Core.Descriptors;

public interface IFlatArguments
{
    public ParameterSource? UnflaggedArgument { get; }
    public ParameterSource? GetArgument(string rawName);
}
