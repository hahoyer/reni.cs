using System;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;
using Stx.DataTypes;

namespace Stx.Tests
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