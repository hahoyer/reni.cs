using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;

namespace ReniUI.Formatting;

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
        StopByObjectIds(382);
    }

    IEnumerable<Edit> IEditPieces.Get(IEditPiecesConfiguration parameter)
    {
        var currentIndent = parameter.Indent;
        parameter.Indent += Count;
        var result = Target.GetEditPieces(parameter);
        parameter.Indent = currentIndent;
        return result;
    }

    ISourcePartEdit ISourcePartEdit.Indent(int count)
    {
        var newCount = count + Count;
        return newCount == 0? Target : new IndentedSourcePartEdit(Target, newCount);
    }

    bool ISourcePartEdit.IsIndentTarget => Target.IsIndentTarget;

    SourcePart ISourcePartEdit.SourcePart => Target.SourcePart;
}