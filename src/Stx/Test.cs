using hw.DebugFormatter;
using hw.UnitTest;

namespace Stx
{
    [UnitTest]
    public sealed class Test
    {
        [UnitTest]
        public void TestMethod()
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
            var c = Compiler.FromText(x);
            var s = c.Syntax;
            Tracer.Assert(s != s);
        }
    }
}