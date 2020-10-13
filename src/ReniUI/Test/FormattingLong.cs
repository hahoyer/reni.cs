using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using hw.UnitTest;
using NUnit.Framework;
using Reni.Parser;
using Reni.TokenClasses;
using ReniUI.Helper;

// ReSharper disable StringLiteralTypo

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
            const string text = @"systemdata:
{
    1 type instance () Memory: ((0 type * ('100' to_number_of_base 64)) mutable) instance ();
    ! mutable FreePointer: Memory array_reference mutable;
    repeat: /\ ^ while () then (^ body (), repeat (^));
};

1 = 1 then 2 else 4;
3;
(Text ('H') << 'allo') dump_print";

            var compiler = CompilerBrowser.FromText(text);

            for(var start = 0; start < compiler.Source.Length; start++)
            for(var end = start + 1; end < compiler.Source.Length; end++)
            {
                var span = (compiler.Source + start).Span(end - start);
                var reformat = compiler.Reformat(targetPart: span);
                if(reformat != null)
                {
                    var newCompiler = CompilerBrowser.FromText(reformat);
                    Tracer.Assert(
                        IsEqual(compiler.Syntax, newCompiler.Syntax
                            , IgnoreWhiteSpaces),
                        () => @$"origin: 
{compiler.Syntax.Dump()} 

new ({span.NodeDump}): 
{newCompiler.Syntax.Dump()} 

"
                    );
                }
            }
        }

        static bool IsEqual(Helper.Syntax target, Helper.Syntax other, IComparator differenceHandler)
        {
            if(target.TokenClass.Id != other.TokenClass.Id)
                return false;

            if(target.Left == null && other.Left != null)
                return false;

            if(target.Left != null && other.Left == null)
                return false;

            if(target.Right == null && other.Right != null)
                return false;

            if(target.Right != null && other.Right == null)
                return false;

            if(target.Left != null && !IsEqual(target.Left, other.Left, differenceHandler))
                return false;

            if(target.Right != null && !IsEqual(target.Right, other.Right, differenceHandler))
                return false;

            return CompareWhiteSpaces(target.Token.PrecededWith, other.Token.PrecededWith, differenceHandler);
        }

        static bool CompareWhiteSpaces
            (IEnumerable<IItem> target, IEnumerable<IItem> other, IComparator differenceHandler)
        {
            if(target.Where(item => item.IsComment()).SequenceEqual(other.Where(item => item.IsComment())
                , differenceHandler.WhiteSpaceComparer))
                return true;

            Dumpable.NotImplementedFunction(target.Dump(), other.Dump(), differenceHandler);
            return default;
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

            if(target.ScannerTokenType != other.ScannerTokenType)
                return false;

            if(target.SourcePart.Id == other.SourcePart.Id)
                return true;

            NotImplementedFunction(target, other);
            return default;
        }

        int IEqualityComparer<IItem>.GetHashCode(IItem target)
        {
            NotImplementedFunction(target);
            return default;
        }
    }
}