using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    abstract class Structure : DumpableObject, IStructure
    {
        protected readonly Syntax Syntax;
        protected readonly StructFormatter Parent;

        protected Structure(Syntax syntax, StructFormatter parent)
        {
            Syntax = syntax;
            Parent = parent;
        }

        Syntax IStructure.Syntax => Syntax;

        IEnumerable<ISourcePartEdit> IStructure.GetSourcePartEdits(SourcePart targetPart)
            => GetSourcePartEdits(targetPart);

        protected abstract IEnumerable<ISourcePartEdit> GetSourcePartEdits(SourcePart targetPart);

        [EnableDump]
        protected string FlatResult => Syntax.FlatFormat(Parent.Configuration);
    }
}