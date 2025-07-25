using Reni.Helper;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.TokenClasses.Brackets;

namespace Reni.SyntaxFactory;

sealed class ColonHandler : DumpableObject, IStatementProvider
{
    IStatementSyntax IStatementProvider.Get(BinaryTree target, Factory factory)
    {
        var (item, annotations) = target.Left.CheckForAnnotations();
        var declarer = DeclarerSyntax.Create(item, annotations, factory.Setup, target.Root);
        var value = factory.GetValueSyntax(target.Right!);
        var result = DeclarationSyntax.Create(declarer, value!, Anchor.Create(target));
        return result;
    }

    static(BinaryTree name, (BinaryTree[] anchors, BinaryTree tag)[]? tags) GetNameAndTags(BinaryTree target)
    {
        if(target.TokenClass is ExclamationBoxToken)
        {
            var (name, tags) = GetNameAndTags(target.Left!);
            var attributes = GetAttributes(target);

            if(tags == null)
                return (name, tags);

            NotImplementedFunction(target, "name", name, "tags", tags);
            return default;
        }


        if(target.TokenClass is Definable)
        {
            target.Left.AssertIsNull();
            target.Right.AssertIsNull();
            return (target, null);
        }

        NotImplementedFunction(target);
        return (null, GetDeclarationTags(target))!;
    }

    static(BinaryTree[] anchors, BinaryTree tag)[] GetAttributes(BinaryTree target)
    {
        NotImplementedFunction(target);
        return default!;
    }


    static(BinaryTree?[] anchors, BinaryTree tag)[] GetDeclarationTags(BinaryTree target)
    {
        if(target.Right!.TokenClass is IDeclarationTag)
        {
            target.Right.Right.AssertIsNull();
            target.Right.Left.AssertIsNull();

            var l = GetDeclarationTags(target.Left!);

            return T((T(target, target.Right), target.Right));
        }

        if(target.Right.TokenClass is IRightBracket)
            return AddMainTag(target
                .Right
                .BracketKernel!
                .Center
                .Chain(arg => arg.Left?.Left)
                .Reverse()
                .Select(GetDeclarationTag1), target);

        NotImplementedFunction(target);

        return target
            .Chain(node => node.TokenClass is ExclamationBoxToken? node.Left : null)
            .SelectMany(GetDeclarationTag)
            .ToArray();
    }

    static(BinaryTree?[] anchors, BinaryTree tag)[] AddMainTag
        (IEnumerable<(BinaryTree?[] anchors, BinaryTree tag)> list, BinaryTree target)
    {
        var top = list.First();
        var anchors = T(target, target.Right, target.Right!.Left).Concat(top.anchors).ToArray();
        var result = T((anchors, top.tag)).Concat(list.Skip(1)).ToArray();
        return result;
    }

    static(BinaryTree?[] anchors, BinaryTree tag) GetDeclarationTag1(BinaryTree arg)
    {
        if(arg.Left == null && arg.Right == null)
            return (T(arg), arg);

        if(arg.Right == null)
            return (T(arg, arg.Left), arg);
        NotImplementedFunction(arg);
        return default;
    }

    static(BinaryTree?[] anchors, BinaryTree tag)[] GetDeclarationTag(BinaryTree target)
    {
        target.AssertIsNotNull();
        target.Left.AssertIsNull();

        if(target.Right != null)
            NotImplementedFunction(target);


        var nodes = target
            .Right
            .GetNodesFromLeftToRight()
            .GroupBy(node => node.TokenClass is IDeclarationTag)
            .ToDictionary(group => group.Key, group => group.ToArray());
        var tags = nodes.SingleOrDefault(node => node.Key).Value;
        var result = tags.Select(tag => (anchors: new BinaryTree?[0], tag)).ToArray();
        var other = nodes.SingleOrDefault(node => !node.Key).Value;
        other?.All(item => item.TokenClass is IBracket).Assert();
        result[0].anchors = T(T(target), other).ConcatMany().ToArray();
        return result;
    }
}