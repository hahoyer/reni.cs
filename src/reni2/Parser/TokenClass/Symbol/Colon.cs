namespace Reni.Parser.TokenClass.Symbol
{
	/// <summary>
	/// Summary description for Colon.
	/// </summary>
	internal sealed class Colon : Base
	{
	    internal override Syntax.Base CreateSyntax(Syntax.Base left, Token token, Syntax.Base right)
	    {
	        return left.CreateDeclarationSyntax(token, right);
	    }
	}
}
