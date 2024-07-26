using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Helper;
using Reni.Parser;
using Reni.SyntaxFactory;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.SyntaxTree;

using Annotation = (BinaryTree annotation, BinaryTree[] anchors);

sealed class DeclarerSyntax : DumpableObject
{
    internal sealed class TagSyntax : Syntax.NoChildren
    {
        internal readonly IDeclarationTag Value;

        internal TagSyntax(IDeclarationTag value, Anchor anchor)
            : base(anchor)
        {
            Value = value;
            StopByObjectIds(590);
        }

        protected override IEnumerable<Issue> GetIssues()
        {
            if(Value == null)
                yield return IssueId.InvalidDeclarationTag.GetIssue(Anchor.Main.SourcePart);
        }

        internal override void AssertValid(Level level, BinaryTree target = null)
            => base.AssertValid(level == null? null : new Level { IsCorrectOrder = level.IsCorrectOrder }, target);
    }

    internal sealed class NameSyntax : Syntax.NoChildren
    {
        internal readonly string Value;

        internal NameSyntax(string name, Anchor anchor)
            : base(anchor)
        {
            Value = name;
            StopByObjectIds();
        }

        internal override void AssertValid(Level level, BinaryTree target = null)
            => base.AssertValid(level == null? null : new Level { IsCorrectOrder = level.IsCorrectOrder }, target);
    }

    [EnableDumpExcept(null)]
    internal readonly NameSyntax Name;

    internal readonly TagSyntax[] Tags;

    [EnableDumpExcept(null)]
    internal readonly Syntax.IssueSyntax Issue;

    readonly bool? MeansPublic;

    readonly ValueCache<Syntax[]> DirectChildrenCache;

    [DisableDump]
    internal Syntax[] DirectChildren => DirectChildrenCache.Value;

    [DisableDump]
    internal SourcePart SourcePart
        => T(Tags.SelectMany(node => node.Anchor.SourceParts), Name?.Anchor.SourceParts)
            .ConcatMany()
            .Where(i => i != null)
            .Aggregate();

    [EnableDumpExcept(false)]
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
    internal bool IsMutableSyntax => Tags.Any(item => item.Value is MutableAnnotation);

    [DisableDump]
    internal int DirectChildCount => Tags.Length + 1 + (Issue == null? 0 : 1);

    DeclarerSyntax
    (
        TagSyntax[] tags
        , NameSyntax name
        , Syntax.IssueSyntax issue
        , bool? meansPublic
    )
    {
        tags.AssertIsNotNull();
        Tags = tags;
        Name = name;
        MeansPublic = meansPublic;
        Issue = issue;
        DirectChildrenCache = new(() => DirectChildCount.Select(GetDirectChild).ToArray());
        StopByObjectIds(713);
    }

    protected override string GetNodeDump()
        => base.GetNodeDump()
            + "["
            + (Name?.Value ?? "")
            + Tags.Select(tag => "!" + ((tag?.Value as TokenClass)?.Id ?? "?")).Stringify("")
            + "]";

    internal Syntax GetDirectChild(int index)
    {
        if(index < 0)
            return null;
        if(index < Tags.Length)
            return Tags[index];
        index -= Tags.Length;
        return index switch
        {
            0 => Name, 1 => Issue, _ => null
        };
    }

    internal static DeclarerSyntax Create
    (
        BinaryTree name
        , Annotation[] tags
        , bool meansPublic
    )
    {
        var nameIssueAnchors = name.GetNodesFromLeftToRight().Where(a => a != name).ToArray();

        var nameSyntax = GetNameSyntax(name);

        if(nameIssueAnchors.Length == 0 && tags.Length == 0)
            return new([], nameSyntax, null, meansPublic);

        var issueAnchors = tags
            .Where(i => i.annotation == null)
            .SelectMany(tuple => tuple.anchors)
            .Union(nameIssueAnchors)
            .Where(a => a != null)
            .Distinct()
            .ToArray();

        var issueAnchor = issueAnchors.SourceParts().Combine();
        var issueSyntax = issueAnchor == null
            ? null
            : new Syntax.IssueSyntax
            (
                IssueId.InvalidDeclaration.GetIssue(issueAnchor)
                , Anchor.Create(issueAnchors)
            );

        return new
        (
            tags.Select(GetTagSyntax).Where(i => i != null).ToArray()
            , nameSyntax
            , issueSyntax
            , meansPublic
        );
    }

    static NameSyntax GetNameSyntax(BinaryTree name)
        => name == null? null : new NameSyntax(name.Token.Id, Anchor.Create(name));

    static TagSyntax GetTagSyntax(Annotation target)
    {
        var tag = target.annotation;
        if(tag == null)
            return null;

        var tagToken = tag.TokenClass as DeclarationTagToken;
        return new(tagToken, Anchor.Create(tag).Combine(target.anchors));
    }

    public bool IsDefining(string name, bool publicOnly)
        => name != null && Name?.Value == name && (!publicOnly || IsPublic);
}