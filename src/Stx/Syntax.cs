using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Stx.Forms;
using Stx.TokenClasses;

namespace Stx
{
    sealed class Syntax : DumpableObject, ISourcePartProxy
    {
        public static Syntax Create(Syntax left, ITokenClass tokenClass, IToken token, Syntax right)
            => new Syntax(left, tokenClass, token, right);


        Syntax(Syntax left, ITokenClass tokenClass, IToken token, Syntax right)
        {
            Left = left;
            TokenClass = tokenClass;
            Token = token;
            Right = right;
        }

        SourcePart ISourcePartProxy.All => SourcePart;

        [EnableDumpExcept(null)]
        internal Syntax Left {get;}

        internal IToken Token {get;}

        [EnableDump]
        internal ITokenClass TokenClass {get;}

        [EnableDumpExcept(null)]
        internal Syntax Right {get;}

        SourcePart SourcePart => Left?.SourcePart + Token.SourcePart() + Right?.SourcePart;
        public IForm Form => TokenClass.GetForm(this);
    }
}