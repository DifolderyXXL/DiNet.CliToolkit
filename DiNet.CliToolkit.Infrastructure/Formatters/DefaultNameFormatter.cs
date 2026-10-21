using DiNet.CliToolkit.Core.Intrerfaces;

namespace DiNet.CliToolkit.Infrastructure.Formatters;

public class DefaultNameFormatter : INameFormatter
{
    public string Format(string rawName)
        => rawName;
}
