using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Helper;
using Reni.SyntaxFactory;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.SyntaxTree;

sealed class DeclarerSyntax : DumpableObject, IItem
{
    internal class TagSyntax : Syntax.NoChildren
    {
        internal readonly IDeclarationTagToken Value;

        internal TagSyntax(IDeclarationTagToken value, Issue issue, Anchor anchor)
            : base(anchor, issue)
        {
            Value = value;
            StopByObjectIds(590);
        }

        internal override void AssertValid(Level level, BinaryTree target = null)
            => base.AssertValid(level == null? null : new Level { IsCorrectOrder = level.IsCorrectOrder }, target);
    }

    internal class NameSyntax : Syntax.NoChildren
    {
        internal readonly string Value;

        internal NameSyntax(string name, Anchor anchor)
            : base(anchor)
            => Value = name;

        internal override void AssertValid(Level level, BinaryTree target = null)
            => base.AssertValid(level == null? null : new Level { IsCorrectOrder = level.IsCorrectOrder }, target);
    }

    internal readonly NameSyntax Name;
    internal readonly TagSyntax[] Tags;
    internal readonly Issue Issue;
    readonly bool? MeansPublic;
    internal readonly Anchor Anchor;

    readonly ValueCache<Syntax[]> DirectChildren;

    DeclarerSyntax
    (
        TagSyntax[] tags
        , NameSyntax name
        , bool? meansPublic
        , Anchor anchor = null
        , Issue issue = null
    )
    {
        Tags = tags;
        Name = name;
        MeansPublic = meansPublic;
        Issue = issue;
        Anchor = anchor;
        DirectChildren = new(() => DirectChildCount.Select(GetDirectChild).ToArray());

        StopByObjectIds();
    }

    Anchor IItem.Anchor => Anchor;
    Syntax[] IItem.DirectChildren => DirectChildren.Value;
    BinaryTree IItem.SpecialAnchor => null;

    protected override string GetNodeDump()
        => base.GetNodeDump() +
            "[" +
            Tags.Select(tag => ((tag?.Value as TokenClass)?.Id ?? "?") + "!").Stringify("") +
            (Name?.Value ?? "") +
            "]";

    internal SourcePart SourcePart
        => T(Tags.SelectMany(node => node.Anchor.SourceParts), Name?.Anchor.SourceParts)
            .ConcatMany()
            .Where(i => i != null)
            .Aggregate();

    internal bool IsPublic
    {
        get
        {
            if(Tags.Any(item => item.Value is PublicDeclarationToken))
                return true;

            if(Tags.Any(item => item.Value is NonPublicDeclarationToken))
                return false;

            return MeansPublic ?? false;
        }
    }

    [DisableDump]
    internal bool IsMixInSyntax => Tags.Any(item => item.Value is MixInDeclarationToken);

    [DisableDump]
    internal bool IsConverterSyntax => Tags.Any(item => item.Value is ConverterToken);

    [DisableDump]
    internal bool IsMutableSyntax => Tags.Any(item => item.Value is MutableDeclarationToken);

    [DisableDump]
    internal int DirectChildCount => Tags.Length + 1;

    internal Syntax GetDirectChild(int index)
    {
        if(index >= 0 && index < Tags.Length)
            return Tags[index];
        return index == Tags.Length? Name : null;
    }

    internal static DeclarerSyntax Create
    (
        (BinaryTree[] anchors, BinaryTree tag)[] tags
        , BinaryTree name
        , bool meansPublic
    )
    {
        var nameSyntax = GetNameSyntax(name);

        if(tags == null)
            return new(new TagSyntax[0], nameSyntax, meansPublic);

        var issueAnchors = tags
            .Where(i => i.tag == null)
            .SelectMany(tuple => tuple.anchors)
            .Distinct()
            .ToArray();

        var anchor = issueAnchors.SingleOrDefault();
        return new
        (
            tags.Select(GetTagSyntax).Where(i => i != null).ToArray()
            , nameSyntax
            , meansPublic
            , anchor == null? null : Anchor.Create(anchor.GetNodesFromLeftToRight().ToArray())
            , anchor == null? null : IssueId.InvalidDeclaration.Issue(anchor.SourcePart)
        );
    }

    static NameSyntax GetNameSyntax(BinaryTree name)
        => name == null? null : new NameSyntax(name.Token.Characters.Id, Anchor.Create(name));

    static TagSyntax GetTagSyntax((BinaryTree[] anchors, BinaryTree tag) target)
    {
        var tag = target.tag;
        if(tag == null)
            return null;

        var tagToken = tag.TokenClass as DeclarationTagToken;
        return new
        (
            tagToken
            , tagToken == null? IssueId.InvalidDeclarationTag.Issue(tag.Token.Characters) : null
            , Anchor.Create(tag).Combine(target.anchors)
        );
    }

    static DeclarerSyntax FromName(BinaryTree target, bool meansPublic, Anchor anchor = null)
    {
        var nameSyntax = new NameSyntax(target.Token.Characters.Id, Anchor.Create(target).Combine(anchor));
        return new(new TagSyntax[0], nameSyntax, meansPublic);
    }

    public bool IsDefining(string name, bool publicOnly)
        => name != null && Name?.Value == name && (!publicOnly || IsPublic);
}