using System.Collections.Generic;
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
            internal readonly IDeclarationTag Value;

            internal TagSyntax(IDeclarationTag value, BinaryTree target)
                : base(target)
                => Value = value;
        }

        internal class NameSyntax : NoChildren
        {
            internal readonly string Value;

            internal NameSyntax([NotNull] BinaryTree target, [NotNull] string name)
                : base(target)
                => Value = name;
        }

        internal static readonly Result<DeclarerSyntax> Empty
            = new Result<DeclarerSyntax>(new DeclarerSyntax(new TagSyntax[0], null));

        internal readonly NameSyntax Name;
        internal readonly TagSyntax[] Tags;

        DeclarerSyntax(TagSyntax[] tags, BinaryTree target, NameSyntax name = null)
            : base(target)
        {
            Tags = tags;
            Name = name;
        }

        protected override IEnumerable<Syntax> GetChildren()
        {
            yield return Name;
            foreach(var tag in Tags)
                yield return tag;
        }

        public static DeclarerSyntax Tag(DeclarationTagToken tag, BinaryTree target)
            => new DeclarerSyntax(new[] {new TagSyntax(tag, target)}, target);

        public DeclarerSyntax WithName(BinaryTree root, string name)
        {
            Tracer.Assert(Name == null);
            return new DeclarerSyntax(Tags, root, new NameSyntax(root, name));
        }

        public DeclarerSyntax Combine(DeclarerSyntax other, BinaryTree root)
        {
            Tracer.Assert(Name == null || other.Name == null);
            return new DeclarerSyntax(Tags.Concat(other.Tags).ToArray(), root, Name ?? other.Name);
        }
    }
}