using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using NUnit.Framework;

namespace Reni.FeatureTest.UserInterface
{
    [UnitTest]
    [TestFixture]
    [LowPriority]
    public sealed class FormattingLong : DependantAttribute
    {
        [Test]
        [UnitTest]
        public void ReformatPart()
        {
            const string Text = @"systemdata:
{
    1 type instance () Memory: ((0 type * ('100' to_number_of_base 64)) mutable) instance ();
    ! mutable FreePointer: Memory array_reference mutable;
    repeat: /\ ^ while () then (^ body (), repeat (^));
};

1 = 1 then 2 else 4;
3;
(Text ('H') << 'allo') dump_print";

            var compiler = Compiler.FromText(text: Text);

            for(var start = 0; start < compiler.Source.Length; start++)
                for(var end = start; end < compiler.Source.Length; end++)
                {
                    var span = (compiler.Source + start).Span(end - start);
                    var reformat = compiler.SourceSyntax.Reformat(span);
                }
        }
    }
}