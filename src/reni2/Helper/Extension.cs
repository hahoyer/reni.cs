using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using JetBrains.Annotations;

namespace Reni.Helper
{
    [PublicAPI]
    static class Extension
    {
        internal static IEnumerable<TTarget> GetNodesFromLeftToRight<TTarget>(this ITree<TTarget> target)
            where TTarget : ITree<TTarget>
        {
            if(target == null)
                yield break;

            for(var index = 0; index < target.DirectChildCount; index++)
            {
                if(index == target.LeftDirectChildCount)
                    yield return (TTarget)target;
                var node = target.GetDirectChild(index);
                if(node != null)
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
                if(index == target.LeftDirectChildCount)
                    yield return (TTarget)target;
                var node = target.GetDirectChild(index);
                if(node != null)
                    foreach(var child in node.GetNodesFromRightToLeft())
                        yield return child;
            }
        }

        static TTarget[] SubBackChain<TTarget>(this TTarget target, TTarget recent)
            where TTarget : class, ITree<TTarget>
        {
            if(target == recent)
                return new TTarget[0];

            return target.DirectChildCount
                .Select(target.GetDirectChild)
                .Where(child => child != null)
                .Select(child => child.BackChain(recent).ToArray())
                .FirstOrDefault(result => result.Any());
        }

        internal static IEnumerable<TTarget> BackChain<TTarget>(this TTarget target, TTarget recent)
            where TTarget : class, ITree<TTarget>
        {
            var subChain = target.SubBackChain(recent);
            if(subChain == null)
                yield break;

            foreach(var items in subChain)
                yield return items;

            yield return target;
        }
    }
}