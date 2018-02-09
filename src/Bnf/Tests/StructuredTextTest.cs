using Bnf.StructuredText;
using hw.DebugFormatter;
using hw.UnitTest;

namespace Bnf.Tests
{
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

            var syntax = c.Syntax;
            Tracer.Line(Tracer.Dump(syntax));
            Tracer.TraceBreak();
        }
    }
}