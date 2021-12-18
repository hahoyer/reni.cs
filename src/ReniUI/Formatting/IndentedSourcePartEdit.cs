using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;

namespace ReniUI.Formatting
{
    sealed class IndentedSourcePartEdit : DumpableObject, ISourcePartEdit, IEditPieces
    {
        [EnableDump]
        internal readonly ISourcePartEdit Target;

        [EnableDump]
        internal readonly int Count;

        public IndentedSourcePartEdit(ISourcePartEdit target, int count)
        {
            Target = target;
            Count = count;
        }

        IEnumerable<Edit> IEditPieces.Get(IEditPiecesConfiguration parameter)
        {
            var currentIndent = parameter.Indent;
            parameter.Indent += Count;
            var result = Target.GetEditPieces(parameter);
            parameter.Indent = currentIndent;
            return result;
        }

        bool ISourcePartEdit.IsIndentTarget => Target.IsIndentTarget;

        ISourcePartEdit ISourcePartEdit.Indent(int count)
        {
            var newCount = count + Count;
            return newCount == 0? Target : new IndentedSourcePartEdit(Target, newCount);
        }

        SourcePart ISourcePartEdit.SourcePart => Target.SourcePart;
    }
}