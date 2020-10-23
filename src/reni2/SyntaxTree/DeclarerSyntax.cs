using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.SyntaxFactory;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.SyntaxTree
{
    sealed class DeclarerSyntax : DumpableObject, IAggregateable<DeclarerSyntax >
    {
        internal class TagSyntax : Syntax.NoChildren
        {
            internal readonly IDeclarationTagToken Value;

            internal TagSyntax
                (IDeclarationTagToken value, BinaryTree anchor, Issue issue, FrameItemContainer frameItems)
                : base(anchor, issue, frameItems)
            {
                Value = value;
                Anchor.AssertIsNotNull();
            }

            [EnableDump]
            [EnableDumpExcept(null)]
            string Position => Anchor?.Token.Characters.GetDumpAroundCurrent(5);

            internal override void AssertValid(Level level, BinaryTree target = null)
                => base.AssertValid(level == null? null : new Level {IsCorrectOrder = level.IsCorrectOrder}, target);
        }

        internal class NameSyntax : Syntax.NoChildren
        {
            internal readonly string Value;

            internal NameSyntax(BinaryTree anchor, string name, FrameItemContainer frameItems)
                : base(anchor, frameItems:frameItems)
            {
                Value = name;
                Anchor.AssertIsNotNull();
                (Value == Anchor.Token.Characters.Id).Assert();
            }

            [EnableDump]
            [EnableDumpExcept(null)]
            string Position => Anchor?.Token.Characters.GetDumpAroundCurrent(5);

            internal override void AssertValid(Level level, BinaryTree target = null)
                => base.AssertValid(level == null? null : new Level {IsCorrectOrder = level.IsCorrectOrder}, target);
        }

        internal static readonly DeclarerSyntax Empty
            = new DeclarerSyntax(null, new TagSyntax[0], null, null);

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
            => T(T(Hidden?.SourcePart), Tags.Select(node => node.SourcePart), T(Name?.SourcePart))
                .ConcatMany()
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

        internal static DeclarerSyntax FromTag
        (
            DeclarationTagToken tag, BinaryTree target, bool? meansPublic, FrameItemContainer frameItems = null
            , Issue issue = null
        )
            => new DeclarerSyntax(null, new[] {new TagSyntax(tag, target, issue, frameItems)}, null, meansPublic);

        internal static DeclarerSyntax FromName
            (BinaryTree target, string name, bool? meansPublic, FrameItemContainer frameItems= null)
            => new DeclarerSyntax(null, new TagSyntax[0], new NameSyntax(target, name, frameItems), meansPublic);

        internal DeclarerSyntax Combine(DeclarerSyntax other)
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
            => base.GetNodeDump() 
                +"["
               + Tags.Select(tag=>(tag?.Value?.NodeDump()??"?")+"!").Stringify("") 
               + (Name?.Value??"")+"]";

        DeclarerSyntax IAggregateable<DeclarerSyntax>.Aggregate(DeclarerSyntax other) => Combine(other);
    }
}