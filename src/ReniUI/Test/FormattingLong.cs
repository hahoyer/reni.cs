using hw.DebugFormatter;
using hw.UnitTest;
using NUnit.Framework;
using Reni.TokenClasses;

namespace ReniUI.Test
{
    [UnitTest]
    [TestFixture]
    [LowPriority]
    public sealed class FormattingLong : DependantAttribute
    {
        static readonly IComparator IgnoreWhiteSpaces = new IgnoreWhiteSpacesComparator();

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

            var compiler = CompilerBrowser.FromText(Text);

            for(var start = 0; start < compiler.Source.Length; start++)
            for(var end = start + 1; end < compiler.Source.Length; end++)
            {
                var span = (compiler.Source + start).Span(end - start);
                var reformat = compiler.Reformat(sourcePart:span);
                if(reformat != null)
                {
                    var newCompiler = CompilerBrowser.FromText(reformat);
                    Tracer.Assert(compiler.Syntax.IsEqual(newCompiler.Syntax, IgnoreWhiteSpaces));
                }
            }
        }
    }

    class IgnoreWhiteSpacesComparator : IComparator {}
}