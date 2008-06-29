using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Syntax;

namespace Reni.Parser.TokenClass.Name
{
	/// <summary>
	/// Summary description for argToken.
	/// </summary>
    sealed internal class TargT : Terminal
	{
        /// <summary>
        /// Visits the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="category">The category.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// [created 13.05.2006 21:53]
	    internal override Result Result(ContextBase context, Category category, Token token)
	    {
            return context.CreateArgsRefResult(category);
        }

	    internal override string DumpShort()
	    {
	        return "arg";
	    }
	}
}
