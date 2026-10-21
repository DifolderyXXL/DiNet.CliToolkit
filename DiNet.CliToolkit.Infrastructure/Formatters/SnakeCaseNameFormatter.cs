using Humanizer;
using DiNet.CliToolkit.Core.Intrerfaces;


namespace DiNet.CliToolkit.Infrastructure.Formatters;

public class SnakeCaseNameFormatter : INameFormatter
{
    public string Format(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;

        return name.Underscore();
    }
}

