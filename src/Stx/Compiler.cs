using hw.Parser;
using hw.Scanner;
using Stx.CodeItems;
using Stx.Contexts;
using Stx.Scanner;
using Stx.TokenClasses;

namespace Stx
{
    sealed class Compiler : Compiler<Syntax>
    {
        public static Compiler FromText(string text) => new Compiler(text);

        static PrioTable PrioTable
        {
            get
            {
                var result = PrioTable.Left(PrioTable.Any);


                result += PrioTable.BracketParallels
                (
                    new[] {LeftBracket.TokenId, LeftParentheses.TokenId},
                    new[] {RightBracket.TokenId, RightParentheses.TokenId}
                );

                result += PrioTable.Right(TokenClasses.Reassign.TokenId);
                result += PrioTable.Right(Colon.TokenId, Semicolon.TokenId);
                result += PrioTable.Right(Of.TokenId, Else.TokenId);
                result += PrioTable.BracketParallels
                (
                    new[] {Case.TokenId},
                    new[] {EndCase.TokenId}
                );

                result += PrioTable.BracketParallels
                (
                    new[] {PrioTable.BeginOfText},
                    new[] {PrioTable.EndOfText}
                );
                result.Title = "Main";
                //Tracer.FlaggedLine("\n"+x.ToString());
                return result;
            }
        }

        readonly string Text;
        CodeItem[] CodeItemsCache;
        internal Context RootContext = Context.Root;

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