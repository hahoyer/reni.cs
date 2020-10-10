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

            for(var index = 0; index < target.DirectChildCount; index++)
            {
                var node = target.GetDirectChild(index);
                if(index == target.LeftDirectChildCount)
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

            for(var index = target.DirectChildCount; index > 0; )
            {
                index--;
                var node = target.GetDirectChild(index);
                if(index == target.LeftDirectChildCount)
                    yield return node;
                else if(node != null)
                    foreach(var child in node.GetNodesFromRightToLeft())
                        yield return child;
            }
        }
    }
}