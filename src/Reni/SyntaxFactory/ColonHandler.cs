using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Helper;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory;

class ColonHandler : DumpableObject, IStatementProvider
{
    IStatementSyntax IStatementProvider.Get(BinaryTree target, Factory factory)
    {
        var declarer = GetDeclarer(target.Left, factory);
        var value = factory.GetValueSyntax(target.Right);

        var result = DeclarationSyntax
            .Create(declarer, value, Anchor.Create(target));
        return result;
    }

    static DeclarerSyntax GetDeclarer(BinaryTree target, Factory factory)
    {
        if(target.TokenClass is Definable)
        {
            target.Right.AssertIsNull();
            return DeclarerSyntax.Create(GetDeclarationTags(target.Left), target, factory.MeansPublic);
        }
        return DeclarerSyntax.Create(GetDeclarationTags(target), null, factory.MeansPublic);
    }

    static (BinaryTree[] anchors, BinaryTree tag)[] GetDeclarationTags(BinaryTree target)
        => target
            .Chain(node => node.TokenClass is ExclamationBoxToken? node.Left : null)
            .SelectMany(GetDeclarationTag)
            .ToArray();

    static(BinaryTree[] anchors, BinaryTree tag)[] GetDeclarationTag(BinaryTree target)
    {
        target.AssertIsNotNull();
        if(target.TokenClass is not ExclamationBoxToken)
            return T<(BinaryTree[] anchors, BinaryTree tag)>((T(target), null));

        target.Right.AssertIsNotNull();
        var nodes = target
            .Right
            .GetNodesFromLeftToRight()
            .GroupBy(node => node.TokenClass is IDeclarationTagToken)
            .ToDictionary(group => group.Key, group => group.ToArray());
        var tags = nodes.SingleOrDefault(node => node.Key).Value;
        var result = tags.Select(tag => (anchors: new BinaryTree[0], tag)).ToArray();
        var issues = nodes.SingleOrDefault(node => !node.Key).Value;
        
        Tracer.ConditionalBreak(issues?.Any()?? false);
        
        result[0].anchors = T(T(target), issues).ConcatMany().ToArray();
        return result;
    }
}