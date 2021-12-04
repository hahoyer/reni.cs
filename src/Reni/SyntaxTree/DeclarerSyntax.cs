using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Helper;
using Reni.SyntaxFactory;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.SyntaxTree
{
    sealed class DeclarerSyntax : DumpableObject
    {
        internal class IssueSyntax : Syntax.NoChildren
        {
            internal IssueSyntax(Issue issue, Anchor anchor)
                : base(anchor, issue) { }
        }

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
        internal readonly IssueSyntax Issue;
        readonly bool? MeansPublic;

        readonly ValueCache<Syntax[]> DirectChildren;

        DeclarerSyntax
        (
            TagSyntax[] tags
            , NameSyntax name
            , IssueSyntax issue
            , bool? meansPublic
        )
        {
            tags.AssertIsNotNull();
            Tags = tags;
            Name = name;
            MeansPublic = meansPublic;
            Issue = issue;
            DirectChildren = new(() => DirectChildCount.Select(GetDirectChild).ToArray());
            StopByObjectIds();
        }

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
        internal int DirectChildCount => Tags.Length + 1 + (Issue == null? 0 : 1);

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
            (BinaryTree[] anchors, BinaryTree tag)[] tags
            , BinaryTree name
            , bool meansPublic
        )
        {
            var nameSyntax = GetNameSyntax(name);

            if(tags == null)
                return new(new TagSyntax[0], nameSyntax, null, meansPublic);

            var issueAnchors = tags
                .Where(i => i.tag == null)
                .SelectMany(tuple => tuple.anchors)
                .Distinct()
                .ToArray();

            var issueAnchor = issueAnchors.SingleOrDefault();
            var issueSyntax = issueAnchor == null
                ? null
                : new IssueSyntax
                (
                    IssueId.InvalidDeclaration.Issue(issueAnchor.SourcePart)
                    , Anchor.Create(issueAnchor.GetNodesFromLeftToRight().ToArray())
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

        public bool IsDefining(string name, bool publicOnly)
            => name != null && Name?.Value == name && (!publicOnly || IsPublic);
    }
}