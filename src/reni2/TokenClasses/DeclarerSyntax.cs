using System.Collections.Generic;
using hw.DebugFormatter;
using Reni.Parser;

namespace Reni.TokenClasses
{
    sealed class DeclarerSyntax : Syntax
    {
        internal class TagSyntax : Terminal
        {
            internal readonly IDeclarationTag Value;

            internal TagSyntax(IDeclarationTag value, BinaryTree target)
                : base(target)
                => Value = value;
        }

        internal class NameSyntax : Terminal
        {
            internal readonly string Value;

            internal NameSyntax(BinaryTree target, string name)
                : base(target)
                => Value = name;
        }

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
    }
}