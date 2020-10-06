using System.Collections.Generic;
using Reni.Parser;

namespace Reni.TokenClasses
{
    abstract class DeclarerSyntax : Syntax
    {
        public class Tag : DeclarerSyntax
        {
            readonly IDeclarationTag[] Tags;

            public Tag(IDeclarationTag tag, BinaryTree target)
                : base(target)
                => Tags = T(tag);

            protected override IEnumerable<Syntax> GetChildren() => new Syntax[0];
        }

        DeclarerSyntax(BinaryTree target)
            : base(target) { }

        protected DeclarerSyntax(int objectId, BinaryTree target)
            : base(objectId, target) { }
    }
}