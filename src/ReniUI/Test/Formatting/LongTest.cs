using hw.DebugFormatter;
using hw.UnitTest;
using Reni.TokenClasses;

// ReSharper disable StringLiteralTypo

namespace ReniUI.Test.Formatting;

[UnitTest]
[TestFixture]
[LowPriority]
public sealed class LongTest : DependenceProvider
{
    [Test]
    [UnitTest]
    public void ReformatPart()
    {
        return;
        const string text = @"systemdata:
{
    1 type instance () Memory: ((0 type * ('100' to_number_of_base 64)) mutable) instance ();
    ! mutable FreePointer: Memory array_reference mutable;
    repeat: @ ^ while () then (^ body (), repeat (^));
};

1 = 1 then 2 else 4;
3;
(Text ('H') << 'allo') dump_print";

        var compiler = CompilerBrowser.FromText(text.Replace("\r\n", "\n"));

        for(var start = 0; start < compiler.Source.Length; start++)
        {
            (start - compiler.Source.Length).LogDump().Log();
            for(var end = start + 1; end < compiler.Source.Length; end++)
            {
                var span = (compiler.Source + start).Span(end - start);
                var reformat = compiler.Reformat(targetPart: span);
                if(reformat != null)
                {
                    var newCompiler = CompilerBrowser.FromText(reformat);
                    Compare(compiler.Compiler.BinaryTree, newCompiler.Compiler.BinaryTree).Assert(() => @$"origin: 
{compiler.Syntax.Dump()} 

new ({span.NodeDump}): 
{newCompiler.Syntax.Dump()} 

"
                    );
                }
            }
        }
    }

    static bool Compare(BinaryTree target, BinaryTree other)
    {
        if(target == null)
            return other == null;

        if(other == null)
            return false;

        if(target.TokenClass.Id != other.TokenClass.Id)
            return false;

        if(target.Token.Id != other.Token.Id)
            return false;

        if(!Compare(target.Left, other.Left))
            return false;

        if(!Compare(target.Right, other.Right))
            return false;

        return true;
    }
}