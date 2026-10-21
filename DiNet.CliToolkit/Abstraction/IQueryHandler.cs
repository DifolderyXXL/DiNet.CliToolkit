using DiNet.CliToolkit.Abstraction;

namespace DiNet.CliToolkit.Abstraction;


public interface IQuery<TResponse>;
public interface IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<TResponse> Handle(TQuery query, CancellationToken ct); 
}
