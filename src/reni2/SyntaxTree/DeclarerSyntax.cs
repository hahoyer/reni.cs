using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Parser;
using Reni.SyntaxFactory;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.SyntaxTree
{
    sealed class DeclarerSyntax : DumpableObject, IAggregateable<DeclarerSyntax>
    {
        internal class TagSyntax : Syntax.NoChildren
        {
            internal readonly IDeclarationTagToken Value;

            internal TagSyntax
                (IDeclarationTagToken value, Issue issue, Anchor anchor)
                : base(anchor, issue)
            {
                Value = value;
            }

            [EnableDump]
            [EnableDumpExcept(null)]
            string Position => Anchor.SourceParts.DumpSource(5);

            internal override void AssertValid(Level level, BinaryTree target = null)
                => base.AssertValid(level == null? null : new Level {IsCorrectOrder = level.IsCorrectOrder}, target);
        }

        internal class NameSyntax : Syntax.NoChildren
        {
            internal readonly string Value;

            internal NameSyntax(string name, Anchor anchor)
                : base(anchor: anchor)
                => Value = name;

            [EnableDump]
            [EnableDumpExcept(null)]
            string Position => Anchor.SourceParts.DumpSource(5);

            internal override void AssertValid(Level level, BinaryTree target = null)
                => base.AssertValid(level == null? null : new Level {IsCorrectOrder = level.IsCorrectOrder}, target);
        }

        internal readonly NameSyntax Name;
        internal readonly TagSyntax[] Tags;

        readonly DeclarerSyntax Hidden;
        readonly bool? MeansPublic;

        DeclarerSyntax(DeclarerSyntax hidden, TagSyntax[] tags, NameSyntax name, bool? meansPublic)
        {
            Hidden = hidden;
            Tags = tags;
            Name = name;
            MeansPublic = meansPublic;

            StopByObjectIds();
        }

        internal Issue Issue => Hidden == null? null : IssueId.StrangeDeclaration.Issue(Hidden.SourcePart);

        internal SourcePart SourcePart
            => T(T(Hidden?.SourcePart), Tags.SelectMany(node => node.Anchor.SourceParts), Name?.Anchor.SourceParts)
                .ConcatMany()
                .Where(i=>i!= null)
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

        DeclarerSyntax IAggregateable<DeclarerSyntax>.Aggregate(DeclarerSyntax other) => Combine(other);

        internal Syntax GetDirectChild(int index)
        {
            if(index >= 0 && index < Tags.Length)
                return Tags[index];
            return index == Tags.Length? Name : null;
        }

        internal static DeclarerSyntax GetDeclarationTag(BinaryTree target, bool meansPublic, Anchor frameItems)
        {
            (target.Right == null).Assert();
            switch(target.TokenClass)
            {
                case DeclarationTagToken _:
                case InvalidDeclarationError _:
                    return FromTag(target, meansPublic, frameItems);
                case Definable _: 
                    return FromName(target, meansPublic, frameItems);
                default:
                    NotImplementedFunction(target, frameItems);
                    return default;
            }
        }

        static DeclarerSyntax FromTag(BinaryTree target, bool meansPublic, Anchor frameItems)
        {
            var tagSyntax = GetTagSyntax(target, frameItems);
            return new DeclarerSyntax(null, new[] {tagSyntax}, null, meansPublic);
        }

        static TagSyntax GetTagSyntax(BinaryTree target, Anchor anchor)
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

        protected override string GetNodeDump()
            => base.GetNodeDump() +
               "[" +
               Tags.Select(tag => ((tag?.Value as TokenClass)?.Id ?? "?") + "!").Stringify("") +
               (Name?.Value ?? "") +
               "]";
    }
}