using hw.Parser;
using hw.Scanner;

namespace Stx
{
    sealed class Syntax : ISourcePartProxy
    {
        public static Syntax CreateSourceSyntax
            (Syntax left, TokenClass tokenClass, IToken token, Syntax right) =>
            new Syntax(left, tokenClass, token, right);

        public readonly Syntax Left;
        public readonly Syntax Right;
        readonly IToken Token;
        readonly TokenClass TokenClass;

        Syntax(Syntax left, TokenClass tokenClass, IToken token, Syntax right)
        {
            Left = left;
            TokenClass = tokenClass;
            Token = token;
            Right = right;
        }

        SourcePart ISourcePartProxy.All => SourcePart;

        SourcePart SourcePart => Left?.SourcePart + Token.SourcePart() + Right?.SourcePart;
    }
}