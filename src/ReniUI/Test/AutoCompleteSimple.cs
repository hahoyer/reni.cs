using hw.DebugFormatter;
using hw.Scanner;
using hw.UnitTest;
using NUnit.Framework;

namespace ReniUI.Test
{
    [UnitTest]
    [TestFixture]
    public sealed class AutoCompleteSimple : DependantAttribute
    {
        const string text = @"

    NewMemory: /\
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
            for(var i = 0; i < text.Length; i++)
            {
                var offset = text.Length - i - 1;
                var position = compiler.Source + offset;
                if(offset > 0 && (position + -1).Span(2).Id != "\r\n")
                {
                    var t = compiler.DeclarationOptions(offset);
                    Tracer.Assert(t != null, () => (new Source(text) + i).Dump());
                }
            }
        }

    }
}