using DiNet.CliToolkit.Core.Models;

namespace DiNet.CliToolkit.Core.Intrerfaces;

public interface ICommandHandlerFactory
{
    T Create<T>(CommandCallRequest request);
    object Create(Type type, CommandCallRequest request);
}
