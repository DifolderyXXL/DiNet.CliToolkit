using DiNet.CliToolkit.Core;
using DiNet.CliToolkit.Core.Intrerfaces;

namespace DiNet.CliToolkit.Infrastructure;

public class CommandCallerStore : ICommandCallerStore
{
    private readonly Dictionary<Type, CommandMetadata> _callers = [];
    public CommandMetadata? Get<T>()
    {
        if (_callers.TryGetValue(typeof(T), out var caller))
            return caller;

        return null;
    }

    public CommandMetadata? Get(Type type)
    {
        if (_callers.TryGetValue(type, out var caller))
            return caller;

        return null;
    }
}
