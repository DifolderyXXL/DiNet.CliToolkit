using DiNet.CliToolkit.Core;
using DiNet.CliToolkit.Core.Models;

namespace DiNet.CliToolkit.Core.Intrerfaces;

public interface IParameterBinder
{
    object?[] Bind(CommandCaller caller, CommandCallRequest request);
}
