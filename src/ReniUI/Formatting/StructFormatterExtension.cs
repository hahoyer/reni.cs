using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    static class StructFormatterExtension
    {
        internal interface IStructure
        {
            IEnumerable<Edit> GetEditPieces(SourcePart targetPart);
        }

        class ParenthesisStructure : DumpableObject, IStructure
        {
            [EnableDump]
            readonly Syntax Syntax;

            public ParenthesisStructure(Syntax syntax) => Syntax = syntax;

            IEnumerable<Edit> IStructure.GetEditPieces(SourcePart targetPart)
            {
                NotImplementedMethod(targetPart);
                return null;
            }
        }

        internal static IStructure CreateStruct(this Syntax syntax)
        {
            if(syntax.TokenClass is RightParenthesis)
                return new ParenthesisStructure(syntax);
            Dumpable.NotImplementedFunction(syntax);
            return null;
        }
    }
}