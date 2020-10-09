using System.Collections.Generic;

namespace Reni.Helper
{
    static class Extension
    {
        internal static IEnumerable<TTarget> GetNodesFromLeftToRight<TTarget>(this ITree<TTarget> target)
            where TTarget : ITree<TTarget>
        {
            if(target == null)
                yield break;

            for(var index = 0; index < target.DirectNodeCount; index++)
            {
                var node = target.GetDirectNode(index);
                if(ReferenceEquals(node, target))
                    yield return node;
                else if(node != null)
                    foreach(var child in node.GetNodesFromLeftToRight())
                        yield return child;
            }
        }

        internal static IEnumerable<TTarget> GetNodesFromRightToLeft<TTarget>(this ITree<TTarget> target)
            where TTarget : ITree<TTarget>
        {
            if(target == null)
                yield break;

            for(var index = target.DirectNodeCount; index > 0; )
            {
                index--;
                var node = target.GetDirectNode(index);
                if(ReferenceEquals(node, target))
                    yield return node;
                else if(node != null)
                    foreach(var child in node.GetNodesFromRightToLeft())
                        yield return child;
            }
        }
    }
}