using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
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
                    parameter.SpaceCount++;
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
}