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
        internal static readonly ISourcePartEdit EnsureSeparator = new SpecialEdit("EnsureSeparator");
        internal static readonly ISourcePartEdit IndentStart = new SpecialEdit("IndentStart");
        internal static readonly ISourcePartEdit IndentEnd = new SpecialEdit("IndentEnd");
        internal static readonly ISourcePartEdit EndOfFile = new SpecialEdit("EndOfFile");

        internal static IEnumerable<Edit> GetEditPieces
            (this IEnumerable<ISourcePartEdit> target, SourcePart targetPart, Configuration configuration)
        {
            var currentPosition = 0;
            var currentIndex = 0;

            var parameter = new EditPieceParameter(configuration);

            var result = new List<Edit>();
            foreach(var part in target)
            {
                if(part == IndentStart)
                    parameter.Indent++;
                else if(part == IndentEnd)
                    --parameter.Indent;
                else if(part == LineBreak)
                {
                    parameter.LineBreakCount++;
                    parameter.IsSeparatorRequired = false;
                }
                else if(part == EnsureSeparator)
                    parameter.IsSeparatorRequired = true;
                else if(part == EndOfFile)
                    parameter.IsEndOfFile = true;
                else if(part is SourcePartEdit spe)
                {
                    var edits = spe.GetEditPiece(parameter).ToArray();
                    foreach(var edit in edits)
                    {
                        Tracer.Assert(currentPosition <= edit.Location.Position);
                        currentPosition = edit.Location.EndPosition;
                        result.Add(edit);
                    }

                    parameter.Reset();
                }
                else
                    Dumpable.NotImplementedFunction(target.ToArray(), targetPart, configuration);

                currentIndex++;
            }

            return result;
        }

        internal static IEnumerable<ISourcePartEdit>
            IndentRight(this IEnumerable<ISourcePartEdit> target)
            => new[] {IndentStart}.Concat(target).Concat(new[] {IndentEnd});

        internal static IEnumerable<ISourcePartEdit>
            IndentLeft(this IEnumerable<ISourcePartEdit> target)
            => new[] {IndentEnd}.Concat(target).Concat(new[] {IndentStart});
    }
}