using System;
using hw.DebugFormatter;
using hw.UnitTest;

namespace Bnf.Tests
{
    [UnitTest]
    public sealed class Test
    {
        [UnitTest]
        public void TestMethod()
        {
            var x = @"
letter ::= 'A' | 'B' | <...> | 'Z' | 'a' | 'b' | <...> | 'z';
digit ::= '0' | '1' | '2' | '3' | '4' | '5' | '6' | '7' | '8' | '9';
octal_digit ::= '0' | '1' | '2' | '3' | '4' | '5' | '6' | '7';
hex_digit ::= digit | 'A'|'B'|'C'|'D'|'E'|'F';
identifier ::= (letter | ('_' (letter | digit))) {['_'] (letter | digit)};
";
            var c = Compiler.FromText(x);

            var form = c.Syntax.Form;
            Tracer.Line(Tracer.Dump(form));
            var s = c.CodeItems;
            Tracer.Assert(s != s, () => Tracer.Dump(s));
            throw new NotImplementedException();
        }
    }
}