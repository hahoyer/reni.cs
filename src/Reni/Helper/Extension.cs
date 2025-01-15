namespace Reni.Helper;

public static class Extension
{
    [PublicAPI]
    internal static IEnumerable<TTarget?> GetDirectChildren<TTarget>(this ITree<TTarget?> target)
        where TTarget : ITree<TTarget>
        => target.GetDirectChildrenInternal();

    static IEnumerable<TTarget?> GetDirectChildrenInternal<TTarget>(this ITree<TTarget?> target)
        where TTarget : ITree<TTarget>
    {
        for(var index = 0; index < target.DirectChildCount; index++)
            yield return target.GetDirectChild(index);
    }

    [PublicAPI]
    internal static IEnumerable<TTarget?> GetNodesFromTopToBottom<TTarget>
        (this ITree<TTarget?> target, Func<TTarget?, bool>? predicate = null)
        where TTarget : ITree<TTarget>
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

    [PublicAPI]
    internal static IEnumerable<TTarget> GetNodesFromLeftToRight<TTarget>(this ITree<TTarget?>? target)
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

    [PublicAPI]
    internal static int[]? GetPath<TTarget>(this TTarget? container, Func<TTarget?, bool> isMatch)
        where TTarget : ITree<TTarget?>?
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

    [PublicAPI]
    internal static IEnumerable<int[]> GetPaths<TTarget>(this TTarget container, Func<TTarget?, bool> isMatch)
        where TTarget : ITree<TTarget>?
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

    [PublicAPI]
    internal static IEnumerable<TTarget> GetNodesFromRightToLeft<TTarget>(this ITree<TTarget?>? target)
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

    static TValue[] T<TValue>(params TValue[] value) => value;

    [PublicAPI]
    internal static IEnumerable<TResult?> GetNodesFromLeftToRight<TAspect, TResult>
        (this TResult? target, Func<TResult?, TAspect> getAspect)
        where TAspect : ITree<TResult>
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
    internal static IEnumerable<TResult?> GetNodesFromRightToLeft<TAspect, TResult>
        (this TResult? target, Func<TResult?, TAspect> getAspect)
        where TAspect : ITree<TResult>
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