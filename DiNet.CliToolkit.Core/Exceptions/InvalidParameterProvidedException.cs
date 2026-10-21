using DiNet.CliToolkit.Core.Exceptions;

namespace DiNet.CliToolkit.Core.Exceptions;

[Serializable]
public class InvalidParameterProvidedException : IncorrectCommandStructureException
{
    public InvalidParameterProvidedException()
    {
        
    }
    public InvalidParameterProvidedException(string message) : base(message)
    {
        
    }
}
