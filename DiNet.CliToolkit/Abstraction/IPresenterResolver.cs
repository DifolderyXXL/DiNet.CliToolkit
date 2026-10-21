using DiNet.CliToolkit.Abstraction;
namespace DiNet.CliToolkit.Abstraction;

public interface IPresenterResolver
{
    IUntypedDataPresenter? TryResolve(Type responseType);
}
