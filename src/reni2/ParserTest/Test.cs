using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.ParserTest
{
    [UnitTest]
    public sealed class ParserTest : CompilerTest
    {
        [UnitTest]
        public void SimpleFunction()
        {
            var syntaxPrototype = LikeSyntax.Expression(null, "f", LikeSyntax.Null);
            Parameters.ParseOnly = true;
            CreateFileAndRunCompiler
                (
                    "SimpleFunction",
                    @"f()",
                    expectedResult: c => syntaxPrototype.AssertLike(c.Syntax));
        }

        [UnitTest]
        public void Add2Numbers()
        {
            var syntaxPrototype =
                (LikeSyntax.Number(2) + LikeSyntax.Number(4)).dump_print;
            Parameters.ParseOnly = true;

            CreateFileAndRunCompiler
                (
                    "Add2Numbers",
                    @"(2+4) dump_print",
                    expectedResult: c => syntaxPrototype.AssertLike(c.Syntax));
        }

        [UnitTest]
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

        [UnitTest]
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

        [UnitTest]
        public void LotsOfBrackets()
        {
            var syntaxPrototype =
                LikeSyntax
                    .Expression(null, "<<", LikeSyntax.Number(5))
                    .Expression(null, LikeSyntax.Number(3))
                    .dump_print;
            Parameters.ParseOnly = true;
            Parameters.TraceOptions.Parser = false;
            CreateFileAndRunCompiler
                (
                    "Add2Numbers",
                    @"((<< 5)(3)) dump_print",
                    expectedResult: c => syntaxPrototype.AssertLike(c.Syntax));
        }

        [UnitTest]
        public void MoreBrackets()
        {
            var syntaxPrototype =
                LikeSyntax
                    .Expression(null, "this", LikeSyntax.Null)
                    .Expression(null, LikeSyntax.Number(3))
                    .dump_print;
            Parameters.ParseOnly = true;
            Parameters.TraceOptions.Parser = false;
            CreateFileAndRunCompiler
                (
                    "MoreBrackets",
                    @"this()(3) dump_print",
                    expectedResult: c => syntaxPrototype.AssertLike(c.Syntax));
        }
    }
}