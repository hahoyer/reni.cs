using System.CodeDom;
using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;
using hw.UnitTest;
using NUnit.Framework;
using Reni.TokenClasses;

namespace ReniUI.Test
{
    [UnitTest]
    [TestFixture]
    [LowPriority]
    public sealed class FormattingLong : DependenceProvider
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
                var reformat = compiler.Reformat(targetPart:span);
                if(reformat != null)
                {
                    var newCompiler = CompilerBrowser.FromText(reformat);
                    Tracer.Assert(compiler.BinaryTreeSyntax.Target.IsEqual(newCompiler.BinaryTreeSyntax.Target, IgnoreWhiteSpaces),
                        ()=>@$"origin: 
{compiler.BinaryTreeSyntax.Target.Dump()} 

new ({span.NodeDump}): 
{newCompiler.BinaryTreeSyntax.Target.Dump()} 

"
                    );
                }
            }
        }
    }

    class IgnoreWhiteSpacesComparator : DumpableObject, IComparator, IEqualityComparer<IItem>
    {
        IEqualityComparer<IItem> IComparator.WhiteSpaceComparer => this;

        bool IEqualityComparer<IItem>.Equals(IItem target, IItem other)
        {
            if(target == null)
                return other == null;

            if(other == null)
                return false;

            if(target.ScannerTokenType != other.ScannerTokenType )
                return false;

            if(target.SourcePart.Id == other.SourcePart.Id)
                return true;

            NotImplementedFunction(target,other);
            return default;
        }

        int IEqualityComparer<IItem>.GetHashCode(IItem target)
        {
            NotImplementedFunction(target);
            return default;
        }
    }
}