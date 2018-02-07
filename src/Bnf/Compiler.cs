using Bnf.CodeItems;
using Bnf.Contexts;
using Bnf.Scanner;
using Bnf.TokenClasses;
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


                result += PrioTable.Right(TokenClasses.Reassign.TokenId);
                result += PrioTable.Right(Colon.TokenId, Semicolon.TokenId);
                result += PrioTable.Right(Of.TokenId, Else.TokenId);

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

                result.Title = "Main";
                //Tracer.FlaggedLine("\n"+x.ToString());
                return result;
            }
        }

        internal Context RootContext = Context.Root;

        readonly string Text;
        CodeItem[] CodeItemsCache;

        Source SourceCache;
        Syntax SyntaxCache;

        Compiler(string text)
        {
            Text = text;

            var main = this["Main"];
            var tokenFactory = new TokenFactory("Main");

            main.PrioTable = PrioTable;
            main.TokenFactory = new ScannerTokenFactory();
            main.Add<ScannerTokenType<Syntax>>(tokenFactory);
        }

        internal Source Source => SourceCache ?? (SourceCache = new Source(Text));
        internal Syntax Syntax => SyntaxCache ?? (SyntaxCache = GetSyntax());
        internal CodeItem[] CodeItems => CodeItemsCache ?? (CodeItemsCache = GetCodeItems());

        CodeItem[] GetCodeItems() => Syntax.Form.GetResult(RootContext).CodeItems;

        Syntax GetSyntax() => this["Main"].Parser.Execute(Source + 0);
    }
}