namespace Reni.Helper;

public static class Extension
{
    static int[]? GetSubPath<TTarget>(TTarget container, int index, Func<TTarget?, bool> isMatch)
        where TTarget : ITree<TTarget>?
    {
        var subPath = GetPath(container, isMatch);
        return subPath == null? null : T(index).Concat(subPath).ToArray();
    }

    static IEnumerable<int[]?> GetSubPaths<TTarget>(TTarget container, int index, Func<TTarget?, bool> isMatch)
        where TTarget : ITree<TTarget>?
    {
        var subPath = GetPaths(container, isMatch);
        return subPath.Select(subPath => T(index).Concat(subPath).ToArray());
    }

    static TValue[] T<TValue>(params TValue[] value) => value;

    extension<TTarget>(ITree<TTarget?>? target)
        where TTarget : ITree<TTarget>
    {
        [PublicAPI]
        internal IEnumerable<TTarget> GetNodesFromLeftToRight()
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

        [PublicAPI]
        internal IEnumerable<TTarget> GetNodesFromRightToLeft()
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
    }
    
    extension<TTarget>(ITree<TTarget?> target)
        where TTarget : ITree<TTarget>
    {
        [PublicAPI]
        internal IEnumerable<TTarget?> GetDirectChildren() => target.GetDirectChildrenInternal();

        IEnumerable<TTarget?> GetDirectChildrenInternal()
        {
            for(var index = 0; index < target.DirectChildCount; index++)
                yield return target.GetDirectChild(index);
        }

        [PublicAPI]
        internal IEnumerable<TTarget?> GetNodesFromTopToBottom(Func<TTarget?, bool>? predicate = null)
        {
            var index = 0;
            while(index < target.DirectChildCount)
            {
                var node = target.GetDirectChild(index);
                if(predicate == null || predicate(node))
                    yield return node;
                else if(node != null)
                    foreach(var result in node.GetNodesFromTopToBottom(predicate))
                        yield return result;
                index++;
            }
        }

    }

    extension<TTarget>(TTarget? container)
        where TTarget : ITree<TTarget?>?
    {
        [PublicAPI]
        internal int[]? GetPath(Func<TTarget?, bool> isMatch)
        {
            if(container == null)
                return null;

            if(isMatch(container))
                return [];

            return container
                .DirectChildCount
                .Select(container.GetDirectChild)
                .Select((node, index) =>  GetSubPath(node, index, isMatch))
                .FirstOrDefault(path => path != null);
        }
    }

    extension<TTarget>(TTarget container)
        where TTarget : ITree<TTarget>?
    {
        [PublicAPI]
        internal IEnumerable<int[]> GetPaths(Func<TTarget?, bool> isMatch)
        {
            if(container == null)
                return new int[0][];

            if(isMatch(container))
                return [[]];

            return container
                .DirectChildCount
                .Select(container.GetDirectChild)
                .SelectMany((node, index) => GetSubPaths(node, index, isMatch))
                .Where(path => path != null)
                .Cast<int[]>();
        }
    }

    extension<TAspect, TResult>(TResult? target)
        where TAspect : ITree<TResult>
    {
        [PublicAPI]
        internal IEnumerable<TResult?> GetNodesFromLeftToRight(Func<TResult?, TAspect> getAspect)
        {
            var aspect = getAspect(target);
            var index = 0;
            var right = new List<TResult?>();
            while(true)
            {
                if(index == aspect.LeftDirectChildCount)
                    right.Add(target);
                if(index == aspect.DirectChildCount)
                    return right;
                var node = aspect.GetDirectChild(index);
                if(node != null)
                    right.AddRange(node.GetNodesFromLeftToRight(getAspect));
                index++;
            }
        }

        [PublicAPI]
        internal IEnumerable<TResult?> GetNodesFromRightToLeft(Func<TResult?, TAspect> getAspect)
        {
            var aspect = getAspect(target);
            var index = aspect.DirectChildCount;
            while(true)
            {
                if(index == aspect.LeftDirectChildCount)
                    yield return target;
                if(index == 0)
                    yield break;
                index--;
                var node = aspect.GetDirectChild(index);
                if(node != null)
                    foreach(var child in node.GetNodesFromRightToLeft(getAspect))
                        yield return child;
            }
        }
    }
}