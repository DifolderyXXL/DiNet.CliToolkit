using DiNet.CliToolkit.Core.Descriptors;
using DiNet.CliToolkit.Core.Intrerfaces;
using DiNet.CliToolkit.Core.Models;

namespace DiNet.CliToolkit.Infrastructure;

public sealed class FlatArgumentArray(string? firstUnflagged, IReadOnlyList<ConsoleParameter> parameters, INameFormatter formatter) : IFlatArguments
{
    public ParameterSource? UnflaggedArgument =>
        firstUnflagged is null ? null : new ParameterSource(null, firstUnflagged);

    public ParameterSource? GetArgument(string rawName) =>
        parameters.FirstOrDefault(p => p.Name == formatter.Format(rawName)) is { } match
            ? new ParameterSource(match.Name, match.Value)
            : null;
}
