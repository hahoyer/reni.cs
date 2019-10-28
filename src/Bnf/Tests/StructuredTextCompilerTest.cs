using Bnf.StructuredText;
using hw.DebugFormatter;
using hw.UnitTest;

namespace Bnf.Tests
{
    [UnitTest]
    public sealed class StructuredTextCompilerTest
    {
        [UnitTest]
        public void BnfTest()
        {
            var definition = BnfDefinitions.Text;

            var compiler = Compiler.FromText(definition);

            var form = compiler.Syntax.Form;
            Tracer.Line(Tracer.Dump(form));
            Tracer.TraceBreak();
        }

    }
}