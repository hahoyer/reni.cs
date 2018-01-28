using System;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Stx.CodeItems;
using Stx.Features;
using Stx.TokenClasses;

namespace Stx
{
    sealed class Syntax : DumpableObject, ISourcePartProxy
    {
        public static Syntax Create(Syntax left, ITokenClass tokenClass, IToken token, Syntax right)
            => new Syntax(left, tokenClass, token, right);

        readonly FunctionCache<Context, Result> ResultCache;

        Syntax(Syntax left, ITokenClass tokenClass, IToken token, Syntax right)
        {
            Left = left;
            TokenClass = tokenClass;
            Token = token;
            Right = right;

            ResultCache = new FunctionCache<Context, Result>(context => TokenClass.GetResult(context, Left, Token, Right));
        }

        SourcePart ISourcePartProxy.All => SourcePart;

        [EnableDumpExcept(null)]
        internal Syntax Left {get;}

        IToken Token {get;}

        [EnableDump]
        internal ITokenClass TokenClass {get;}

        [EnableDumpExcept(null)]
        internal Syntax Right {get;}

        SourcePart SourcePart => Left?.SourcePart + Token.SourcePart() + Right?.SourcePart;

        [DisableDump]
        public CodeItem[] CodeItems => ResultCache[Context.Root].CodeItems;

        public Result GetResult(Context context) => ResultCache[context];
    }
}