using hw.Parser;
using hw.Scanner;

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
                    new[] {PrioTable.BeginOfText},
                    new[] {PrioTable.EndOfText}
                );
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
            main.Add<ScannerTokenType<Syntax>>(tokenFactory);
        }

        internal Source Source => SourceCache ?? (SourceCache = new Source(Text));
        internal Syntax Syntax => SyntaxCache ?? (SyntaxCache = GetSyntax());

        Syntax GetSyntax() => this["Main"].Parser.Execute(Source + 0);
    }
}