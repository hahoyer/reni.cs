using System.Collections.Generic;
using Reni.Parser;

namespace Reni.TokenClasses
{
    class FrameSyntax: ValueSyntax
    {
        readonly ValueSyntax Content;
        internal FrameSyntax(ValueSyntax content, BinaryTree target):base(target) => Content = content;
        protected override IEnumerable<Syntax> GetChildren() => Content.Children;
    }
}