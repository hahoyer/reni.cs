using System.Collections.Generic;
using System.Linq;
using Bnf.Forms;
using Bnf.Scanner;
using Bnf.TokenClasses;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;

namespace Bnf
{
    sealed class Compiler : Compiler<Syntax>
    {
        public static Compiler FromText(string text) => new Compiler(text);

        static PrioTable PrioTable
        {
            get
            {
                var result = PrioTable.Left(PrioTable.Any);
                result += PrioTable.Left(TokenClasses.Or.TokenId);

                result += PrioTable.BracketParallels
                (
                    new[]
                    {
                        LeftParenthesis.TokenId(3),
                        LeftParenthesis.TokenId(2),
                        LeftParenthesis.TokenId(1),
                        PrioTable.BeginOfText
                    },
                    new[]
                    {
                        RightParenthesis.TokenId(3),
                        RightParenthesis.TokenId(2),
                        RightParenthesis.TokenId(1),
                        PrioTable.EndOfText
                    }
                );

                result += PrioTable.Right(TokenClasses.Define.TokenId);
                result += PrioTable.Right(Semicolon.TokenId);
                result.Title = "Main";
                //Tracer.FlaggedLine("\n"+x.ToString());
                return result;
            }
        }

        readonly string Text;

        Source SourceCache;
        Syntax SyntaxCache;

        Compiler(string text)
        {
            Text = text;

            var main = this["Main"];
            var tokenFactory = new TokenFactory("Main");

            main.PrioTable = PrioTable;
            main.TokenFactory = new ScannerTokenFactory();
            main.Add<ITokenTypeFactory>(tokenFactory);
        }

        [DisableDump]
        internal Source Source => SourceCache ?? (SourceCache = new Source(Text));

        [DisableDump]
        internal Syntax Syntax => SyntaxCache ?? (SyntaxCache = GetSyntax());

        [DisableDump]
        public IDictionary<string, IExpression> Statements 
            => ((IStatements) Syntax.Form).Data.ToDictionary(i=>i.Name, i=>i.Value);

        Syntax GetSyntax() => this["Main"].Parser.Execute(Source + 0);
    }

}