using hw.DebugFormatter;
using hw.Scanner;
using hw.UnitTest;
using NUnit.Framework;

namespace ReniUI.Test
{
    [UnitTest]
    [TestFixture]
    [LowPriority]
    public sealed class AutoCompleteSimple : DependenceProvider
    {
        const string text = @"

    NewMemory: @
    (

        result: 1
    )
    result
";

        [UnitTest]
        [Test]
        public void GetDeclarationOptions()
        {
            var compiler = CompilerBrowser.FromText(text);
            for(var index = 0; index < text.Length; index++)
            {
                var offset = text.Length - index - 1;
                var position = compiler.Source + offset;
                if(offset > 0 && (position + -1).Span(2).Id != "\r\n")
                {
                    var t = compiler.DeclarationOptions(offset);
                    Tracer.Assert(t != null, () => (new Source(text) + index).Dump());
                }
            }
        }
    }
}