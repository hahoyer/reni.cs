using System;
using Reni.Context;
using Reni.Parser;

namespace Reni.Syntax
{
    [Serializable]
    internal sealed class EmptyList : CompileSyntax
    {
        public EmptyList(Token token) : base(token) {}

        internal protected override Result Result(ContextBase context, Category category)
        {
            return Type.Void.CreateResult(category);
        }

        internal protected override string DumpShort()
        {
            return "()";
        }
    }
}