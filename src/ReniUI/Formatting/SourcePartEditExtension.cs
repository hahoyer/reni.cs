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
                Dumpable.NotImplementedFunction(target.ToArray(), targetPart, configuration);
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
                    parameter.Space++;
                else if(part is SourcePartEdit spe)
                {
                    result.AddRange(spe.GetEditPieces(parameter));
                    parameter.Reset();
                }
                else
                {
                    Dumpable.NotImplementedFunction(target.ToArray(), targetPart, configuration);
                    return null;
                }

            return result;
        }
    }

    class EditPieceParameter : DumpableObject
    {
        readonly Configuration Configuration;
        public int Indent;
        public int LineBreakCount;
        public int Space;

        public EditPieceParameter(Configuration configuration) => Configuration = configuration;

        Edit Create(SourcePosn span)
        {
            var lineBreakText = "\n".Repeat(LineBreakCount);
            var indentText = LineBreakCount > 0 ? " ".Repeat(Indent * Configuration.IndentCount) : "";
            var spaceText = " ".Repeat(Space);

            return new Edit
            {
                Location = span.Span(0),
                NewText = lineBreakText + indentText + spaceText
            };
        }

        public void Reset()
        {
            LineBreakCount = 0;
            Space = 0;
        }

        public IEnumerable<Edit> GetEditPieces(SourcePosn sourcePosn, int lineBreakCount, int spaceCount)
        {
            if(lineBreakCount == 0 && spaceCount == 0)
            {
                if(LineBreakCount == 0 && Space == 0)
                    return new Edit[0];
                return new[] {Create(sourcePosn)};
            }

            NotImplementedMethod(sourcePosn, lineBreakCount, spaceCount);
            return null;
        }
    }
}