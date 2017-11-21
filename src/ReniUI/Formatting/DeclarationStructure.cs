using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    class DeclarationStructure : DumpableObject, IStructure
    {
        readonly StructFormatter Parent;
        readonly Syntax Syntax;

        public DeclarationStructure(Syntax syntax, StructFormatter parent)
        {
            Syntax = syntax;
            Parent = parent;
        }

        IEnumerable<ISourcePartEdit> IStructure.GetSourcePartEdits(SourcePart targetPart)
        {
            NotImplementedMethod(targetPart);
            return null;
        }

        Syntax IStructure.Syntax => Syntax;
    }
}