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
        readonly BinaryTree SpecialAnchor;
        readonly Anchor Anchor;

        readonly ValueCache<Syntax[]> DirectChildren;

        DeclarerSyntax(DeclarerSyntax hidden, TagSyntax[] tags, NameSyntax name, bool? meansPublic, BinaryTree specialAnchor = null)
        {
            Hidden = hidden;
            Tags = tags;
            Name = name;
            MeansPublic = meansPublic;
            SpecialAnchor = specialAnchor;


            Anchor = Anchor.Create(T(hidden?.Anchor.Items, T(specialAnchor)).ConcatMany());

            DirectChildren = new ValueCache<Syntax[]>(() => DirectChildCount.Select(GetDirectChild).ToArray());

            StopByObjectIds();
        }


        DeclarerSyntax IAggregateable<DeclarerSyntax>.Aggregate(DeclarerSyntax other) => Combine(other);

        Anchor IItem.Anchor => Anchor;
        BinaryTree IItem.SpecialAnchor => SpecialAnchor;
        Syntax[] IItem.DirectChildren => DirectChildren.Value;

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

        internal static DeclarerSyntax GetDeclarationTag(BinaryTree target, bool meansPublic, Anchor frameItems)
            => target.TokenClass switch
            {
                Definable _ => FromName(target, meansPublic, frameItems), _ => FromTag(target, meansPublic, frameItems)
            };

        internal static DeclarerSyntax Create
            (BinaryTree[] tags, BinaryTree[] names, bool meansPublic, BinaryTree specialAnchor)
        {
            (names == null || names.Length < 2).Assert("To do");//todo: Error handling if more than one name is provided
            
            var tagSyntax = (tags??new BinaryTree[0]).Select(tag=>GetTagSyntax(tag)).ToArray();
            var nameSyntax = GetNameSyntax(names);
            return new DeclarerSyntax(null, tagSyntax, nameSyntax, meansPublic, specialAnchor);
        }

        static NameSyntax GetNameSyntax(BinaryTree[] names)
        {
            if(names == null || !names.Any())
                return null;
            var name = names[0];
            return new NameSyntax(name.Token.Characters.Id, Anchor.Create(name));
        }

        static DeclarerSyntax FromTag(BinaryTree target, bool meansPublic, Anchor frameItems)
        {
            var tagSyntax = GetTagSyntax(target, frameItems);
            return new DeclarerSyntax(null, new[] {tagSyntax}, null, meansPublic);
        }

        static TagSyntax GetTagSyntax(BinaryTree target, Anchor anchor= null)
        {
            var tag = target.TokenClass as DeclarationTagToken;
            var issue = tag == null? IssueId.InvalidDeclarationTag.Issue(target.Token.Characters) : null;
            return new TagSyntax(tag, issue, Anchor.Create(target).Combine(anchor));
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