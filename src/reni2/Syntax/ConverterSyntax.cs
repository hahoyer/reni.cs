using Reni.Parser;
using Reni.Parser.TokenClass;

namespace Reni.Syntax
{
    internal sealed class ConverterSyntax : SyntaxBase
    {
        private readonly SyntaxBase _body;
        private readonly Token _token;

        internal ConverterSyntax(SyntaxBase body, Token token)
        {
            _body = body;
            _token = token;
        }

        internal SyntaxBase Body { get { return _body; } }

        internal override SyntaxBase CreateListSyntaxReverse(SyntaxBase left, Token token)
        {
            return CreateListSyntax(this).CreateListSyntaxReverse(left, token);
        }

        internal override SyntaxBase CreateListSyntaxReverse(DeclarationSyntax left, Token token)
        {
            return CreateListSyntax(this).CreateListSyntaxReverse(left, token);
        }

        /// <summary>
        /// Dumps the short.
        /// </summary>
        /// <returns></returns>
        /// created 07.05.2007 22:09 on HAHOYER-DELL by hh
        internal override string DumpShort()
        {
            return "converter (" + _body.DumpShort() + ")";
        }
    }
}