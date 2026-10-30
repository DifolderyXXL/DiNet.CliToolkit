namespace DiNet.CliToolkit.Common.Graphs;

public static class GraphTraverseHelper<TNode, TKey>
{
    public static void TraverseAcyclicGraph(TKey root, FunctionalGraphNode<TNode, TKey> graph)
    {
        TraverseAcyclicGraph(root, graph, []);
    }

    private static void TraverseAcyclicGraph(TKey root, FunctionalGraphNode<TNode, TKey> graph, HashSet<TKey> path)
    {
        var currentNode = graph.NodeProvider(root);

        if (!path.Add(root))
            throw new AcyclicGraphInvariantException("Cycle detected.");

        try
        {
            var children = graph.ChildrenProvider(currentNode);
            foreach (var t in children)
            {
                if (!graph.IsLeaf(t))
                    TraverseAcyclicGraph(t, graph, path);
            }

            graph.ChildrenTraversed?.Invoke(currentNode, children);
        }
        finally
        {
            path.Remove(root);
        }
    }

    public static void TraverseUniqueKeyGraph(TKey root, FunctionalGraphNode<TNode, TKey> graph)
    {
        TraverseUniqueKeyGraph(root, graph, []);
    }

    private static void TraverseUniqueKeyGraph(TKey root, FunctionalGraphNode<TNode, TKey> graph, HashSet<TKey> path)
    {
        var currentNode = graph.NodeProvider(root);

        if (!path.Add(root))
            throw new UniqueKeyGraphInvariantException();

        var children = graph.ChildrenProvider(currentNode);
        foreach (var t in children)
        {
            if (!graph.IsLeaf(t))
                TraverseAcyclicGraph(t, graph, path);
        }

        graph.ChildrenTraversed?.Invoke(currentNode, children);
    }
}