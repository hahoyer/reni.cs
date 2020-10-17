using System.Linq;
using hw.DebugFormatter;
using JetBrains.Annotations;
using Reni.Parser;

namespace Reni.TokenClasses
{
    sealed class DeclarerSyntax : Syntax
    {
        internal class TagSyntax : NoChildren
        {
            internal readonly SyntaxFactory.IDeclarerToken Value;

            internal TagSyntax(SyntaxFactory.IDeclarerToken value, BinaryTree anchor)
                : base(anchor)
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

        internal class NameSyntax : NoChildren
        {
            internal readonly string Value;

            internal NameSyntax([NotNull] BinaryTree anchor, [NotNull] string name)
                : base(anchor)
                => Value = name;

            [EnableDump]
            [EnableDumpExcept(null)]
            string Position => Anchor?.Token.Characters.GetDumpAroundCurrent(5);

            internal override void AssertValid(Level level, BinaryTree target = null)
                => base.AssertValid(level == null? null : new Level {IsCorrectOrder = level.IsCorrectOrder}, target);
        }

        internal static readonly Result<DeclarerSyntax> Empty
            = new Result<DeclarerSyntax>(new DeclarerSyntax(new TagSyntax[0], null, null, null));

        internal readonly NameSyntax Name;
        internal readonly TagSyntax[] Tags;
        readonly bool? MeansPublic;

        DeclarerSyntax(TagSyntax[] tags, BinaryTree anchor, NameSyntax name, bool? meansPublic)
            : base(anchor)
        {
            Tags = tags;
            Name = name;
            MeansPublic = meansPublic;

            anchor.AssertIsNull();
            StopByObjectIds();
        }

        public bool IsPublic
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
        internal override int LeftDirectChildCount => Tags.Length;

        [DisableDump]
        protected override int DirectChildCount => Tags.Length + 1;

        [EnableDump]
        [EnableDumpExcept(null)]
        string Position => Anchor?.Token.Characters.GetDumpAroundCurrent(5);

        internal override void AssertValid(Level level, BinaryTree target = null)
            => base.AssertValid(level == null? null : new Level {IsCorrectOrder = level.IsCorrectOrder}, target);

        protected override Syntax GetDirectChild(int index)
        {
            if(index >= 0 && index < Tags.Length)
                return Tags[index];
            return index == Tags.Length? Name : null;
        }

        internal static DeclarerSyntax FromTag(DeclarationTagToken tag, BinaryTree target, bool? meansPublic)
            => new DeclarerSyntax(new[] {new TagSyntax(tag, target)}, null, null, meansPublic);

        internal static DeclarerSyntax FromName(BinaryTree target, string name, bool? meansPublic)
            => new DeclarerSyntax(new TagSyntax[0], null, new NameSyntax(target, name), meansPublic);

        internal DeclarerSyntax Combine(DeclarerSyntax other, BinaryTree root = null)
        {
            if(other == null)
                return this;
            Tracer.Assert(Name == null || other.Name == null);
            Tracer.Assert(MeansPublic == null || other.MeansPublic == null || MeansPublic == other.MeansPublic);
            return new DeclarerSyntax(Tags.Concat(other.Tags).ToArray(), root, Name ?? other.Name
                , MeansPublic ?? other.MeansPublic);
        }

        public bool IsDefining(string name, bool publicOnly)
            => name != null && Name?.Value == name && (!publicOnly || IsPublic);
    }
}