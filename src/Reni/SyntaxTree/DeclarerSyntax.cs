using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.SyntaxFactory;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.SyntaxTree
{
    sealed class DeclarerSyntax : DumpableObject, IAggregateable<DeclarerSyntax>, IItem
    {
        internal class TagSyntax : Syntax.NoChildren
        {
            internal readonly IDeclarationTagToken Value;

            internal TagSyntax(IDeclarationTagToken value, Issue issue, Anchor anchor)
                : base(anchor, issue)
                => Value = value;

            internal override void AssertValid(Level level, BinaryTree target = null)
                => base.AssertValid(level == null? null : new Level {IsCorrectOrder = level.IsCorrectOrder}, target);
        }

        internal class NameSyntax : Syntax.NoChildren
        {
            internal readonly string Value;

            internal NameSyntax(string name, Anchor anchor)
                : base(anchor)
                => Value = name;

            internal override void AssertValid(Level level, BinaryTree target = null)
                => base.AssertValid(level == null? null : new Level {IsCorrectOrder = level.IsCorrectOrder}, target);
        }

        internal readonly NameSyntax Name;
        internal readonly TagSyntax[] Tags;

        readonly DeclarerSyntax Hidden;
        readonly bool? MeansPublic;
        readonly BinaryTree SpecialFormattingAnchor;
        readonly Anchor Anchor;

        readonly ValueCache<Syntax[]> DirectChildren;

        DeclarerSyntax
        (
            DeclarerSyntax hidden, TagSyntax[] tags, NameSyntax name, bool? meansPublic
            , BinaryTree specialFormattingAnchor = null
        )
        {
            Hidden = hidden;
            Tags = tags;
            Name = name;
            MeansPublic = meansPublic;
            SpecialFormattingAnchor = specialFormattingAnchor;


            Anchor = Anchor.Create(T(hidden?.Anchor.Items, T(specialFormattingAnchor)).ConcatMany());

            DirectChildren = new ValueCache<Syntax[]>(() => DirectChildCount.Select(GetDirectChild).ToArray());

            StopByObjectIds();
        }


        DeclarerSyntax IAggregateable<DeclarerSyntax>.Aggregate(DeclarerSyntax other) => Combine(other);

        Anchor IItem.Anchor => Anchor;
        Syntax[] IItem.DirectChildren => DirectChildren.Value;
        BinaryTree IItem.SpecialAnchor => SpecialFormattingAnchor;

        protected override string GetNodeDump()
            => base.GetNodeDump() +
                "[" +
                Tags.Select(tag => ((tag?.Value as TokenClass)?.Id ?? "?") + "!").Stringify("") +
                (Name?.Value ?? "") +
                "]";

        internal Issue Issue => Hidden == null? null : IssueId.StrangeDeclaration.Issue(Hidden.SourcePart);

        internal SourcePart SourcePart
            => T(T(Hidden?.SourcePart), Tags.SelectMany(node => node.Anchor.SourceParts), Name?.Anchor.SourceParts)
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
        internal int DirectChildCount => (Hidden?.DirectChildCount ?? 0) + Tags.Length + 1;

        internal Syntax GetDirectChild(int index)
        {
            if(index >= 0 && index < Tags.Length)
                return Tags[index];
            return index == Tags.Length? Name : null;
        }

        internal static DeclarerSyntax Create
        (
            (BinaryTree[] Anchors, BinaryTree tag)[] tags, BinaryTree name, bool meansPublic
            , BinaryTree specialFormattingAnchor
        )
        {
            var tagSyntax = new TagSyntax[0];
            if(tags != null)
                tagSyntax = tags.Select(GetTagSyntax).ToArray();
            var nameSyntax = GetNameSyntax(name);
            return new DeclarerSyntax(null, tagSyntax, nameSyntax, meansPublic, specialFormattingAnchor);
        }

        static NameSyntax GetNameSyntax(BinaryTree name)
            => name == null? null : new NameSyntax(name.Token.Characters.Id, Anchor.Create(name));

        static TagSyntax GetTagSyntax((BinaryTree[] anchors, BinaryTree tag) target)
        {
            var tag = target.tag.TokenClass as DeclarationTagToken;
            var issue = tag == null? IssueId.InvalidDeclarationTag.Issue(target.tag.Token.Characters) : null;
            return new TagSyntax(tag, issue
                , Anchor.Create(target.tag).Combine(target.anchors));
        }

        static DeclarerSyntax FromName(BinaryTree target, bool meansPublic, Anchor anchor = null)
        {
            var nameSyntax = new NameSyntax(target.Token.Characters.Id, Anchor.Create(target).Combine(anchor));
            return new DeclarerSyntax(null, new TagSyntax[0], nameSyntax, meansPublic);
        }

        DeclarerSyntax Combine(DeclarerSyntax other)
        {
            if(other == null)
                return this;

            if(Name != null)
                return new DeclarerSyntax(this, other.Tags, other.Name, other.MeansPublic);

            Name.AssertIsNull();
            (MeansPublic == null || other.MeansPublic == null || MeansPublic == other.MeansPublic).Assert();
            return new DeclarerSyntax(Hidden, Tags.Concat(other.Tags).ToArray(), other.Name
                , MeansPublic ?? other.MeansPublic);
        }

        public bool IsDefining(string name, bool publicOnly)
            => name != null && Name?.Value == name && (!publicOnly || IsPublic);
    }
}