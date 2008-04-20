using HWClassLibrary.Debug;

namespace Reni.Parser.TokenClass.Name
{
	/// <summary>
	/// Summary description for argToken.
	/// </summary>
    sealed internal class TargT : Special
	{
        /// <summary>
        /// Creates the syntax.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="token">The token.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 31.03.2007 14:02 on SAPHIRE by HH
        internal override Syntax.SyntaxBase CreateSyntax(Syntax.SyntaxBase left, Token token, Syntax.SyntaxBase right)
        {
            return CreateSpecialSyntax(left, token, right);
        }

        /// <summary>
        /// Visits the specified context.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="token">The token.</param>
        /// <param name="right">The right.</param>
        /// <param name="context">The context.</param>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// [created 13.05.2006 21:53]
	    internal override Result Result(Context.ContextBase context, Category category, Syntax.SyntaxBase left, Token token, Syntax.SyntaxBase right)
	    {
            Tracer.Assert(left == null);
            Tracer.Assert(right == null);
            return context.CreateArgsRefResult(category);
        }
	}
}
