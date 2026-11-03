using DiNet.CliToolkit.Core.Descriptors;

namespace DiNet.CliToolkit.Services.Arguments;

public interface IFlatArgumentsStore
{
    void Set(IFlatArguments arguments);
}
