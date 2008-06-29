using Reni.Context;
using Reni.Parser;

namespace Reni.Syntax
{
    internal sealed class Void : SyntaxBase
    {
        /// <summary>
        /// Visitor function, that ensures correct alignment
        /// This function shoud be called by cache elments only
        /// </summary>
        /// <param name="context">Environment used for deeper visit and alignment</param>
        /// <param name="category">Categories</param>
        /// <returns></returns>
        //[DebuggerHidden]
        public Result VirtVisit(ContextBase context, Category category)
        {
            return Type.Void.CreateResult(category);
        }
    }

    internal sealed class EmptyList : ParsedSyntax
    {
        public EmptyList(Token token) : base(token) {}

        internal protected override string DumpShort()
        {
            return "()";
        }
    }
}