using DiNet.CliToolkit.Core.Descriptors;
using DiNet.CliToolkit.Core.Intrerfaces;
using DiNet.CliToolkit.Core.Models;

namespace DiNet.CliToolkit.Infrastructure;

public class DefaultCommandCallRequestBuilder(IFlatArgumentsFactory factory) : ICommandCallRequestBuilder
{   
    public IFlatArguments Build(ReadOnlySpan<string> args)
    {
        string? firstUnflagged = null;
        var parameters = new List<ConsoleParameter>();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg.StartsWith("-"))
            {
                var name = arg.TrimStart('-');
                string? value = null;

                var equalsIndex = name.IndexOf('=');
                if (equalsIndex != -1)
                {
                    value = name[(equalsIndex + 1)..];
                    name = name[..equalsIndex];
                }
                else if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                {
                    value = args[++i];
                }

                parameters.Add(new ConsoleParameter(name, value));
            }
            else if (firstUnflagged is null)
            {
                firstUnflagged = arg;
            }
        }

        return factory.Create(firstUnflagged, parameters);
    }
}


public interface IFlatArgumentsFactory
{
    IFlatArguments Create(string? firstUnflagged, IReadOnlyList<ConsoleParameter> parameters);
}

public sealed class FlatArgumentsFactory(INameFormatter formatter) : IFlatArgumentsFactory
{
    public IFlatArguments Create(string? firstUnflagged, IReadOnlyList<ConsoleParameter> parameters)
        => new FlatArgumentArray(firstUnflagged, parameters, formatter);
}