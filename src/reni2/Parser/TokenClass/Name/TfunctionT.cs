using System;
using HWClassLibrary.Debug;

namespace Reni.Parser.TokenClass.Name
{
	/// <summary>
	/// Summary description for functionToken.
	/// </summary>
	internal sealed class TfunctionT: Base
	{
        /// <summary>
        /// Creates the syntax.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="token">The token.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 31.03.2007 14:02 on SAPHIRE by HH
        public override Syntax.Base CreateSyntax(Syntax.Base left, Token token, Syntax.Base right)
        {
            return CreateSpecialSyntax(left, token, right);
        }

        /// <summary>
        /// Visits the specified context.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="context">The context.</param>
        /// <param name="category">The category.</param>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 25.10.2006 20:14
	    internal override Result Result(Syntax.Base left, Token token, Syntax.Base right, Context.Base context, Category category)
	    {
            Tracer.Assert(left == null);
	        return context.CreateFunctionResult(category, right);
	    }
	}
    /// <summary>
    /// Summary description for functionToken.
    /// </summary>
    internal sealed class TpropertyT : Base
    {
        public override Syntax.Base CreateSyntax(Syntax.Base left, Token token, Syntax.Base right)
        {
            return CreateSpecialSyntax(left, token, right);
        }

        /// <summary>
        /// Results the specified token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="context">The context.</param>
        /// <param name="category">The category.</param>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        internal override Result Result(Syntax.Base left, Token token, Syntax.Base right, Context.Base context,
                                        Category category)
        {
            Tracer.Assert(left == null);
            return context.CreatePropertyResult(category, right);
        }
    }
}
