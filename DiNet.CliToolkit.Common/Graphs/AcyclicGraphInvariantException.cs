namespace DiNet.CliToolkit.Common.Graphs;

[Serializable]
public class AcyclicGraphInvariantException : Exception
{
    public AcyclicGraphInvariantException() { }
    public AcyclicGraphInvariantException(string message) : base(message) { }
}



[Serializable]
public class UniqueKeyGraphInvariantException : Exception;