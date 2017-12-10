using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    abstract class Structure : DumpableObject, IStructure
    {
        protected readonly StructFormatter Parent;
        protected readonly Syntax Syntax;

        protected Structure(Syntax syntax, StructFormatter parent)
        {
            Syntax = syntax;
            Parent = parent;
        }

        Syntax IStructure.Syntax => Syntax;

        IEnumerable<ISourcePartEdit> IStructure.GetSourcePartEdits(SourcePart targetPart, bool? exlucdePrefix)
            => GetSourcePartEdits(targetPart, exlucdePrefix);

        [EnableDump]
        protected string FlatResult => Syntax.FlatFormat(Parent.Configuration);

        protected abstract IEnumerable<ISourcePartEdit> GetSourcePartEdits(SourcePart targetPart, bool? exlucdePrefix);
    }
}