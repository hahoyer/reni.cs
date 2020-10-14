using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
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

            var index = 0;
            while(index < target.LeftDirectChildCount)
            {
                var node = target.GetDirectChild(index);
                if(node != null)
                    foreach(var child in node.GetNodesFromLeftToRight())
                        yield return child;
                index++;
            }

            yield return (TTarget)target;

            while(index < target.DirectChildCount)
            {
                var node = target.GetDirectChild(index);
                if(node != null)
                    foreach(var child in node.GetNodesFromLeftToRight())
                        yield return child;
                index++;
            }
        }

        internal static int[] GetPath<TTarget>(this TTarget container, Func<TTarget, bool> isMatch)
            where TTarget : ITree<TTarget>
        {
            if(container == null)
                return null;

            if(isMatch(container))
                return new int[0];
            
            return container
                .DirectChildCount
                .Select(container.GetDirectChild)
                .Select((node, index) => GetSubPath(node, index, isMatch))
                .FirstOrDefault(path => path != null);

        }

        static int[] GetSubPath<TTarget>(TTarget container, int index, Func<TTarget, bool> isMatch)
            where TTarget : ITree<TTarget>
        {
            var subPath = GetPath<TTarget>(container, isMatch);
            return subPath == null? null : T(index).Concat(subPath).ToArray();
        }

        internal static IEnumerable<TTarget> GetNodesFromRightToLeft<TTarget>(this ITree<TTarget> target)
            where TTarget : ITree<TTarget>
        {
            if(target == null)
                yield break;

            var index = target.DirectChildCount;
            while(index > target.LeftDirectChildCount)
            {
                index--;
                var node = target.GetDirectChild(index);
                if(node == null)
                    continue;
                foreach(var child in node.GetNodesFromRightToLeft())
                    yield return child;
            }

            yield return (TTarget)target;

            while(index > 0)
            {
                index--;
                var node = target.GetDirectChild(index);
                if(node == null)
                    continue;
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
        public static TValue[] T<TValue>(params TValue[] value) => value;
    }
}