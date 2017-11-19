using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace ReniUI.Formatting
{
    static class SourcePartEditExtension
    {
        class SpecialEdit : DumpableObject, ISourcePartEdit
        {
            readonly string Id;
            public SpecialEdit(string id) => Id = id;
            public override string ToString() => Id;
            protected override string GetNodeDump() => Id;
        }

        internal static readonly ISourcePartEdit LineBreak = new SpecialEdit("LineBreak");
        internal static readonly ISourcePartEdit Space = new SpecialEdit("Space");
        internal static readonly ISourcePartEdit IndentStart = new SpecialEdit("IndentStart");
        internal static readonly ISourcePartEdit IndentEnd = new SpecialEdit("IndentEnd");

        internal static IEnumerable<Edit> GetEditPieces
            (this IEnumerable<ISourcePartEdit> target, SourcePart targetPart, Configuration configuration)
        {
            if(targetPart.Position > 0)
            {
                Dumpable.NotImplementedFunction(target.ToArray(), targetPart);
                return null;
            }

            var parameter = new EditPieceParameter(configuration);

            var result = new List<Edit>();
            foreach(var part in target)
                if(part == IndentStart)
                    parameter.Indent++;
                else if(part == IndentEnd)
                    --parameter.Indent;
                else if(part == LineBreak)
                    parameter.LineBreakCount++;
                else if(part == Space)
                {
                    Dumpable.NotImplementedFunction(target.ToArray(), targetPart);
                    return Enumerable.Empty<Edit>();
                }
                else if(part is SourcePartEdit spe)
                {
                    result.AddRange(spe.GetEditPieces(parameter));
                    parameter.LineBreakCount = 0;
                }
                else
                {
                    Dumpable.NotImplementedFunction(target.ToArray(), targetPart);
                    return null;
                }

            return result;
        }
    }

    class EditPieceParameter
    {
        readonly Configuration Configuration;
        public int Indent;
        public int LineBreakCount;

        public EditPieceParameter(Configuration configuration) => Configuration = configuration;

        public Edit CreateLine(SourcePosn span)
            => new Edit
            {
                Location = span.Span(0),
                NewText = "\n".Repeat(LineBreakCount) + " ".Repeat(Indent * Configuration.IndentCount)
            };
    }
}