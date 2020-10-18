using System.Linq;
using hw.DebugFormatter;
using JetBrains.Annotations;
using Reni.Parser;
using Reni.SyntaxFactory;
using Reni.SyntaxTree;

namespace Reni.TokenClasses
{
    sealed class DeclarerSyntax : DumpableObject
    {
        internal class TagSyntax : Syntax.NoChildren
        {
            internal readonly IDeclarerToken Value;

            internal TagSyntax(IDeclarerToken value, BinaryTree anchor)
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

        internal class NameSyntax : Syntax.NoChildren
        {
            internal readonly string Value;

            internal NameSyntax([NotNull] BinaryTree anchor, [NotNull] string name)
                : base(anchor)
            {
                Value = name;
                Anchor.AssertIsNotNull();
            }

            [EnableDump]
            [EnableDumpExcept(null)]
            string Position => Anchor?.Token.Characters.GetDumpAroundCurrent(5);

            internal override void AssertValid(Level level, BinaryTree target = null)
                => base.AssertValid(level == null? null : new Level {IsCorrectOrder = level.IsCorrectOrder}, target);
        }

        internal static readonly Result<DeclarerSyntax> Empty
            = new Result<DeclarerSyntax>(new DeclarerSyntax(new TagSyntax[0], null, null));

        internal readonly NameSyntax Name;
        internal readonly TagSyntax[] Tags;
        readonly bool? MeansPublic;

        DeclarerSyntax(TagSyntax[] tags, NameSyntax name, bool? meansPublic)
        {
            Tags = tags;
            Name = name;
            MeansPublic = meansPublic;

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
        internal int DirectChildCount => Tags.Length + 1;

        internal  Syntax GetDirectChild(int index)
        {
            if(index >= 0 && index < Tags.Length)
                return Tags[index];
            return index == Tags.Length? Name : null;
        }

        internal static DeclarerSyntax FromTag(DeclarationTagToken tag, BinaryTree target, bool? meansPublic)
            => new DeclarerSyntax(new[] {new TagSyntax(tag, target)}, null, meansPublic);

        internal static DeclarerSyntax FromName(BinaryTree target, string name, bool? meansPublic)
            => new DeclarerSyntax(new TagSyntax[0], new NameSyntax(target, name), meansPublic);

        internal DeclarerSyntax Combine(DeclarerSyntax other)
        {
            if(other == null)
                return this;
            (Name == null || other.Name == null).Assert();
            (MeansPublic == null || other.MeansPublic == null || MeansPublic == other.MeansPublic).Assert();
            return new DeclarerSyntax(Tags.Concat(other.Tags).ToArray(), Name ?? other.Name
                , MeansPublic ?? other.MeansPublic);
        }

        public bool IsDefining(string name, bool publicOnly)
            => name != null && Name?.Value == name && (!publicOnly || IsPublic);
    }
}