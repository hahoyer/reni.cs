using Bnf.Forms;
using Bnf.TokenClasses;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;

namespace Bnf
{
    sealed class Syntax : DumpableObject, ISourcePartProxy
    {
        public static Syntax Create(Syntax left, TokenClasses.ITokenType tokenType, IToken token, Syntax right)
            => new Syntax(left, tokenType, token, right);


        Syntax(Syntax left, TokenClasses.ITokenType tokenType, IToken token, Syntax right)
        {
            Left = left;
            TokenType = tokenType;
            Token = token;
            Right = right;
            StopByObjectIds(553);
        }

        SourcePart ISourcePartProxy.All => SourcePart;

        [EnableDumpExcept(null)]
        internal Syntax Left {get;}

        internal IToken Token {get;}

        [EnableDump]
        internal TokenClasses.ITokenType TokenType {get;}

        [EnableDumpExcept(null)]
        internal Syntax Right {get;}

        [DisableDump]
        SourcePart SourcePart => Left?.SourcePart + Token.SourcePart() + Right?.SourcePart;

        [DisableDump]
        public IForm Form => TokenType.GetForm(this);
    }
}