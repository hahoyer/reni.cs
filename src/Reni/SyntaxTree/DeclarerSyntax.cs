using hw.Scanner;
using Reni.Context;
using Reni.Helper;
using Reni.Parser;
using Reni.SyntaxFactory;
using Reni.TokenClasses;
using Reni.TokenClasses.Brackets;
using Reni.Validation;

namespace Reni.SyntaxTree;

sealed class DeclarerSyntax : DumpableObject
{
    internal sealed class TagSyntax : Syntax.NoChildren
    {
        internal readonly IDeclarationTag? Value;

        internal TagSyntax(IDeclarationTag? value, Anchor anchor)
            : base(anchor)
        {
            Value = value;
            StopByObjectIds(590);
        }

        protected override IEnumerable<Issue> GetIssues()
        {
            if(Value == null)
                yield return IssueId.InvalidDeclarationTag.GetIssue(Anchor.Main.Root, Anchor.Main.SourcePart);
        }

        internal override void AssertValid(Level? level, BinaryTree? target = null)
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

        internal override void AssertValid(Level? level, BinaryTree? target = null)
            => base.AssertValid(level == null? null : new Level { IsCorrectOrder = level.IsCorrectOrder }, target);
    }

    [EnableDumpExcept(null)]
    internal readonly NameSyntax? Name;

    internal readonly TagSyntax[] Tags;

    [EnableDumpExcept(null)]
    internal readonly Syntax.IssueSyntax? Issue;

    readonly TokenClasses.Brackets.Setup Setup;

    readonly ValueCache<Syntax[]> DirectChildrenCache;

    [DisableDump]
    internal Syntax[] DirectChildren => DirectChildrenCache.Value;

    [DisableDump]
    internal SourcePart SourcePart
    {
        get
        {
            IEnumerable<SourcePart?> sourceParts
                = T(Tags.SelectMany(node => node.Anchor.SourceParts), Name?.Anchor.SourceParts)
                    .ConcatMany();
            return sourceParts
                .Where(i => i != null)
                .Aggregate()!;
        }
    }

    [EnableDumpExcept(false)]
    internal bool IsPublic
    {
        get
        {
            if(Tags.Any(item => item.Value is PublicDeclarationToken))
                return true;

            if(Tags.Any(item => item.Value is NonPublicDeclarationToken))
                return false;

            return Setup.MeansPublic;
        }
    }

    [EnableDumpExcept(false)]
    internal bool IsPositional
    {
        get
        {
            if(Tags.Any(item => item.Value is PositionalDeclarationToken))
                return true;

            if(Tags.Any(item => item.Value is NonPositionalDeclarationToken))
                return false;

            return Setup.MeansPositional;
        }
    }

    [EnableDumpExcept(false)]
    internal bool IsKernelPart
    {
        get
        {
            if(Tags.Any(item => item.Value is KernelPartDeclarationToken))
                return true;

            if(Tags.Any(item => item.Value is NonKernelPartDeclarationToken))
                return false;

            return Setup.MeansKernelPart;
        }
    }

    [EnableDumpExcept(false)]
    internal bool IsMixInSyntax => Tags.Any(item => item.Value is MixInDeclarationToken);

    [EnableDumpExcept(false)]
    internal bool IsConverterSyntax => Tags.Any(item => item.Value is ConverterToken);

    [EnableDumpExcept(false)]
    internal bool IsMutableSyntax => Tags.Any(item => item.Value is MutableAnnotation);

    [DisableDump]
    internal int DirectChildCount => Tags.Length + 1 + (Issue == null? 0 : 1);

    DeclarerSyntax
    (
        TagSyntax[] tags
        , NameSyntax? name
        , Syntax.IssueSyntax? issue
        , TokenClasses.Brackets.Setup setup
    )
    {
        Tags = tags;
        Name = name;
        Setup = setup;
        Issue = issue;
        DirectChildrenCache = new(() => DirectChildCount.Select(i => GetDirectChild(i)!).ToArray());
        StopByObjectIds(713);
    }

    protected override string GetNodeDump()
        => base.GetNodeDump()
            + "["
            + (Name?.Value ?? "")
            + Tags.Select(tag => "!" + ((tag.Value as TokenClass)?.Id ?? "?")).Stringify("")
            + "]";

    internal Syntax? GetDirectChild(int index)
    {
        if(index < 0)
            return null;
        if(index < Tags.Length)
            return Tags[index];
        index -= Tags.Length;
        return index switch
        {
            0 => Name, 1 => Issue, var _ => null
        };
    }

    internal static DeclarerSyntax Create
    (
        BinaryTree? name
        , Annotation[] tags
        , TokenClasses.Brackets.Setup setup
        , Root root
    )
    {
        var nameIssueAnchors = name.GetNodesFromLeftToRight().Where(a => a != name).ToArray();

        var nameSyntax = GetNameSyntax(name);

        if(nameIssueAnchors.Length == 0 && tags.Length == 0)
            return new([], nameSyntax, null, setup);

        var issueAnchors = tags
            .Where(i => i.Value == null)
            .SelectMany(tuple => tuple.Anchors)
            .Union(nameIssueAnchors)
            .Distinct()
            .ToArray();

        var issueAnchor = issueAnchors.GetSourceParts().Combine();
        var issueSyntax = issueAnchor == null
            ? null
            : new Syntax.IssueSyntax
            (
                IssueId.InvalidDeclaration.GetIssue(root, issueAnchor)
                , Anchor.Create(issueAnchors)
            );

        return new
        (
            tags.Where(tag=>tag.Value != null).Select(GetTagSyntax).ToArray()
            , nameSyntax
            , issueSyntax
            , setup
        );
    }

    static NameSyntax? GetNameSyntax(BinaryTree? name)
        => name == null? null : new NameSyntax(name.Token.Id, Anchor.Create(name));

    static TagSyntax GetTagSyntax(Annotation target)
    {
        var tag = target.Value!;
        var tagToken = tag.TokenClass as DeclarationTagToken;
        return new(tagToken, Anchor.Create(tag).Combine(target.Anchors));
    }

    internal bool IsDefining(string? name, bool publicOnly)
        => name != null && Name?.Value == name && (!publicOnly || IsPublic);
}