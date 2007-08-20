namespace Reni.Parser.TokenClass.Symbol
{
	/// <summary>
	/// Summary description for Colon.
	/// </summary>
	public class Colon: Base
	{
	    public override Syntax.Base CreateSyntax(Syntax.Base left, Token token, Syntax.Base right)
	    {
	        return left.CreateDeclarationSyntax(token, right);
	    }
	}
}
