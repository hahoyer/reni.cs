using Bnf.StructuredText;
using hw.DebugFormatter;
using hw.UnitTest;

namespace Bnf.Tests
{
    [UnitTest]
    public sealed class StructuredTextCompilerTest
    {
        [UnitTest]
        public void ScannerTest()
        {
            var definition = BnfDefinitions.Scanner;

            var compiler = Compiler.FromText(definition);

            var form = compiler.Syntax.Form;
            Tracer.Line(Tracer.Dump(form));
            Tracer.TraceBreak();
        }


        [UnitTest]
        public void ParserTest()
        {
            string definition = BnfDefinitions.Parser;
            var compiler = Compiler.FromText(definition);

            var form = compiler.Syntax.Form;
            Tracer.Line(Tracer.Dump(form));
            Tracer.TraceBreak();
        }
    }
}