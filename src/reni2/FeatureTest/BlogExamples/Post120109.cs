using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.Parser;

namespace Reni.FeatureTest.BlogExamples
{
    [UnitTest]
    [ParserTest]
    [TargetSet("\"Hello world\"#(* Das ist ein Kommentar *)#dump_print", "Hello world")]
    [TargetSet("\"Hello world\"#(ignorieren Das ist ein Kommentar\n ignorieren)#dump_print", "Hello world")]
    [TargetSet("\"Hello world\"# Das ist der auszugebende String\ndump_print # ... und damit wird er ausgegeben", "Hello world")]
    public sealed class Post120109 : CompilerTest {}
}