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

                result.Title = "Main";
                //Tracer.FlaggedLine("\n"+x.ToString());
                return result;
            }
        }

        readonly string Text;

        Source SourceCache;

        Compiler(string text)
        {
            Text = text;

            var main = this["Main"];
            var tokenFactory = new TokenFactory("Main");

            main.PrioTable = PrioTable;
            main.TokenFactory = new ScannerTokenFactory();
            main.Add<ScannerTokenType<Syntax>>(tokenFactory);
        }

        Source Source => SourceCache ?? (SourceCache = new Source(Text));
    }
}