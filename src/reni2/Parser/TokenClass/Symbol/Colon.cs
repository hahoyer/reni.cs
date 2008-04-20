namespace Reni.Parser.TokenClass.Symbol
{
	/// <summary>
	/// Summary description for Colon.
	/// </summary>
	internal sealed class Colon : Base
	{
	    internal override Syntax.SyntaxBase CreateSyntax(Syntax.SyntaxBase left, Token token, Syntax.SyntaxBase right)
	    {
	        return left.CreateDeclarationSyntax(token, right);
	    }
	}
}
