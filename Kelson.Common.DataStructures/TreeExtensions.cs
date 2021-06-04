using System;
using System.Collections.Generic;

namespace Kelson.Common.DataStructures
{
    public static class TreeExtensions
    {
        public static IEnumerable<TNode> DepthFirstTraverse<TNode>(this TNode startNode, Func<TNode, bool> predicate = null) where TNode : IEnumerable<TNode>
        {
            predicate ??= (n => true);
            var stack = new Stack<(IEnumerator<TNode> children, TNode node)>();
            stack.Push((startNode.GetEnumerator(), startNode));
            if (predicate(startNode))
                yield return startNode;
            while (stack.Count > 0)
            {
                var (children, node) = stack.Pop();
                if (children.MoveNext())
                {
                    stack.Push((children, node));
                    var child = children.Current;
                    if (predicate(child))
                        yield return child;
                    stack.Push((child.GetEnumerator(), child));
                }
            }
        }

        public static IEnumerable<TNode> DepthFirstTraverse<TNode>(this TNode startNode, Func<TNode, IEnumerable<TNode>> childSelector, Func<TNode, bool> predicate = null)
        {
            predicate ??= (n => true);
            var stack = new Stack<(IEnumerator<TNode> children, TNode node)>();
            stack.Push((childSelector(startNode).GetEnumerator(), startNode));
            if (predicate(startNode))
                yield return startNode;
            while (stack.Count > 0)
            {
                var (children, node) = stack.Pop();
                if (children.MoveNext())
                {
                    stack.Push((children, node));
                    var child = children.Current;
                    if (predicate(child))
                        yield return child;
                    stack.Push((childSelector(child).GetEnumerator(), child));
                }
            }
        }

        public static IEnumerable<TNode> DepthFirstTraverse<TNode>(this TNode startNode, Func<TNode, IList<TNode>> childSelector, Func<TNode, bool> predicate = null)
        {
            predicate ??= (n => true);
            var stack = new Stack<(int index, TNode node)>();
            stack.Push((0, startNode));
            if (predicate(startNode))
                yield return startNode;
            while (stack.Count > 0)
            {
                var (index, node) = stack.Pop();
                var children = childSelector(node);
                if (index < children.Count)
                {
                    stack.Push((index + 1, node));
                    var child = children[index];
                    if (predicate(child))
                        yield return child;
                    stack.Push((0, child));
                }
            }
        }

        public static IEnumerable<TNode> DepthFirstTraverse<TNode>(this TNode startNode, Func<TNode, TNode[]> childSelector, Func<TNode, bool> predicate = null)
        {
            predicate ??= (n => true);
            var stack = new Stack<(int index, TNode node)>();
            stack.Push((0, startNode));
            if (predicate(startNode))
                yield return startNode;
            while (stack.Count > 0)
            {
                var (index, node) = stack.Pop();
                var children = childSelector(node);
                if (index < children.Length)
                {
                    stack.Push((index + 1, node));
                    var child = children[index];
                    if (predicate(child))
                        yield return child;
                    stack.Push((0, child));
                }
            }
        }

        public static IEnumerable<TNode> BreadthFirstTraverse<TNode>(this TNode startNode, Func<TNode, bool> predicate = null) where TNode : IEnumerable<TNode>
        {
            predicate ??= (n => true);
            var queue = new Queue<TNode>();
            queue.Enqueue(startNode);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (predicate(node))
                    yield return node;
                foreach (var child in node)
                    queue.Enqueue(child);
            }
        }

        public static IEnumerable<TNode> BreadthFirstTraverse<TNode>(this TNode startNode, Func<TNode, IEnumerable<TNode>> childSelector, Func<TNode, bool> predicate = null)
        {
            predicate ??= (n => true);
            var queue = new Queue<TNode>();
            queue.Enqueue(startNode);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (predicate(node))
                    yield return node;
                foreach (var child in childSelector(node))
                    queue.Enqueue(child);
            }
        }
    }
}
