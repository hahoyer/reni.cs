using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest;
using Reni.FeatureTest.Helper;

namespace Reni.Parser
{
    [TestFixture]
    public sealed class ParserTest : CompilerTest
    {
        public override void Run() { }

        //[Test]
        public void SimpleFunction()
        {
            var syntaxPrototype = LikeSyntax.Expression(null, "f", LikeSyntax.Null);
            Parameters.TraceOptions.Source = true;
            Parameters.ParseOnly = true;
            CreateFileAndRunCompiler("SimpleFunction", @"f()", expectedResult: c => syntaxPrototype.AssertLike(c.Syntax));
        }

        [Test]
        public void Add2Numbers()
        {
            var syntaxPrototype =
                (LikeSyntax.Number(2) + LikeSyntax.Number(4)).dump_print;
            Parameters.ParseOnly = true;
            CreateFileAndRunCompiler
                ("Add2Numbers", @"(2+4) dump_print", expectedResult: c => syntaxPrototype.AssertLike(c.Syntax));
        }

        [Test]
        public void LineComment()
        {
            var syntaxPrototype =
                (LikeSyntax.Number(2) + LikeSyntax.Number(4)).dump_print;
            Parameters.ParseOnly = true;
            CreateFileAndRunCompiler("Add2Numbers", @"
(2+4) # ssssss
dump_print
", expectedResult: c => syntaxPrototype.AssertLike(c.Syntax));
        }

        [Test]
        public void BlockComment()
        {
            var syntaxPrototype =
                (LikeSyntax.Number(2) + LikeSyntax.Number(4)).dump_print;
            Parameters.ParseOnly = true;
            CreateFileAndRunCompiler("Add2Numbers", @"
(2+4) #(aa ssssss
aa)#dump_print
", expectedResult: c => syntaxPrototype.AssertLike(c.Syntax));
        }
    }
}