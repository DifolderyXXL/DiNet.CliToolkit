using DiNet.CliToolkit.Abstraction;
namespace DiNet.CliToolkit.Abstraction;

public interface IQueryHandlerResolver
{
    public IUntypedQueryHandler Resolve(Type type);
}
