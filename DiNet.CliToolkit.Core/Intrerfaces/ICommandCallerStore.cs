using DiNet.CliToolkit.Core;

namespace DiNet.CliToolkit.Core.Intrerfaces;

public interface ICommandCallerStore
{
    public CommandMetadata? Get<T>();
    public CommandMetadata? Get(Type type);
}
