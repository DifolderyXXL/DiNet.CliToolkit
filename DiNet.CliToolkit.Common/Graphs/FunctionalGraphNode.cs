namespace DiNet.CliToolkit.Common.Graphs;

public record class FunctionalGraphNode<TNode, TKey>(
    Func<TNode, TKey> KeyProvider,
    Func<TKey, TNode> NodeProvider,
    Func<TNode, IEnumerable<TKey>> ChildrenProvider,
    Action<TNode, IEnumerable<TKey>>? ChildrenTraversed,
    Predicate<TKey> IsLeaf);
