using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace ReniUI.Formatting
{
    static class SourcePartEditExtension
    {
        sealed class SpecialEdit : DumpableObject, ISourcePartEdit
        {
            readonly string Id;
            public SpecialEdit(string id) => Id = id;
            public override string ToString() => Id;
            protected override string GetNodeDump() => Id;
        }

        internal static readonly ISourcePartEdit LineBreak = new SpecialEdit("LineBreak");
        internal static readonly ISourcePartEdit SpaceRequired = new SpecialEdit("SpaceRequired");
        internal static readonly ISourcePartEdit IndentStart = new SpecialEdit("IndentStart");
        internal static readonly ISourcePartEdit IndentEnd = new SpecialEdit("IndentEnd");
        internal static readonly ISourcePartEdit EndOfFile = new SpecialEdit("EndOfFile");

        internal static IEnumerable<Edit> GetEditPieces
            (this IEnumerable<ISourcePartEdit> target, SourcePart targetPart, Configuration configuration)
        {
            var currentPosition = 0;
            var currentIndex = 0;

            var parameter = new EditPieceParameter(configuration);

            foreach(var part in target)
            {
                if(part == IndentStart)
                    parameter.Indent++;
                else if(part == IndentEnd)
                    --parameter.Indent;
                else if(part == LineBreak)
                {
                    parameter.LineBreakCount++;
                    parameter.IsSpaceRequired = false;
                }
                else if(part == SpaceRequired)
                    parameter.IsSpaceRequired = true;
                else if(part == EndOfFile)
                    parameter.IsEndOfFile = true;
                else if(part is SourcePartEdit spe)
                {
                    var edit = spe.GetEditPiece(parameter);
                    if(edit != null)
                    {
                        Tracer.Assert(currentPosition <= edit.Location.Position);
                        currentPosition = edit.Location.EndPosition;
                        yield return edit;
                    }
                    parameter.Reset();
                }
                else
                    Dumpable.NotImplementedFunction(target.ToArray(), targetPart, configuration);

                currentIndex++;
            }
        }
    }
}