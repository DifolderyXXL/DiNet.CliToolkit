using DiNet.CliToolkit.Core;
using DiNet.CliToolkit.Core.Intrerfaces;

namespace DiNet.CliToolkit.Infrastructure;

public class ReflectionCommandCallerStore(INameFormatter formatter) : ICommandCallerStore
{
    public CommandCaller? Get<T>()
    {
        return CommandCaller.CreateFor(typeof(T), formatter);
    }

    public CommandCaller? Get(Type type)
    {
        return CommandCaller.CreateFor(type, formatter);
    }
}
