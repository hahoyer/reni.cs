using Reni.Parser;

namespace Reni.Syntax
{
    sealed internal class Void : SyntaxBase
    {
        private readonly Token _token;

        internal Void(Token token)
        {
            _token = token;
        }

        internal Void()
        {
        }

        internal Token Token { get { return _token; } }

        /// <summary>
        /// Visitor function, that ensures correct alignment
        /// This function shoud be called by cache elments only
        /// </summary>
        /// <param name="context">Environment used for deeper visit and alignment</param>
        /// <param name="category">Categories</param>
        /// <returns></returns>
        //[DebuggerHidden]
        internal override Result VirtVisit(Context.ContextBase context, Category category)
        {
            return Type.Void.CreateResult(category);
        }

        /// <summary>
        /// Dumps the short.
        /// </summary>
        /// <returns></returns>
        /// created 07.05.2007 22:09 on HAHOYER-DELL by hh
        internal override string DumpShort()
        {
            return "()";
        }
    }
}