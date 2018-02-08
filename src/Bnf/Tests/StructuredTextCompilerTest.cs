using System;
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
            var code = compiler.Interfaces;
            Tracer.Line(code);
            Tracer.TraceBreak();
        }


        [UnitTest]
        public void ParserTest()
        {
            string definition = BnfDefinitions.Parser;
            var compiler = Compiler.FromText(definition);

            var form = compiler.Syntax.Form;
            Tracer.Line(Tracer.Dump(form));
            var code = compiler.Interfaces;
            Tracer.Line(code);
            Tracer.TraceBreak();
        }
    }

    [UnitTest]
    public sealed class StructuredTextTest
    {
        [UnitTest]
        public void Case()
        {
            var x = @"(* simple state machine *)
TxtState := STATES[StateMachine];

CASE StateMachine OF
   1: ClosingValve();
      StateMachine := 2;
   2: OpeningValve();
ELSE
    BadCase();
END_CASE;";
            var c = StructuredText.Compiler.FromText(x);
            c.RootContext =
                c.RootContext
                    .WithVariable("txtstate", DataType.Integer)
                    .WithVariable("StateMachine", DataType.Integer)
                    .WithVariable("STATES", DataType.Integer.Array(10))
                ;

            var form = c.Syntax.Form;
            Tracer.Line(Tracer.Dump(form));
            var s = c.CodeItems;
            Tracer.Assert(s != s, () => Tracer.Dump(s));
            throw new NotImplementedException();
        }
    }
}