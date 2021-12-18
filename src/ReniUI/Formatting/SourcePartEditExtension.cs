using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using JetBrains.Annotations;

namespace ReniUI.Formatting
{
    static class SourcePartEditExtension
    {
        internal static IEnumerable<Edit> GetEditPieces(this IEnumerable<ISourcePartEdit> target)
        {
            var parameter = new EditPieceParameter();

            var sortedTargets = target
                .OrderBy(node => node.SourcePart.Position)
                .ThenBy(node => node.SourcePart.Length)
                .ToArray();
            return sortedTargets
                .SelectMany(part => part.GetEditPieces(parameter))
                .ToArray();
        }

        internal static IEnumerable<Edit> GetEditPieces(this ISourcePartEdit target, IEditPiecesConfiguration parameter)
        {
            switch(target)
            {
                case IEditPieces spe:
                    return spe.Get(parameter).ToArray();

                default:
                    Dumpable.NotImplementedFunction(target, parameter);
                    return null;
            }
        }

        internal static IEnumerable<ISourcePartEdit>
            Indent(this IEnumerable<ISourcePartEdit> target, int direction)
            => direction == 0? target : target.Select(node => node.Indent(direction));

        [UsedImplicitly]
        static IEnumerable<TValue> T<TValue>(params TValue[] value) => value;

        public static ISourcePartEdit CreateIndent(this ISourcePartEdit target, int count)
            => count != 0 && target.IsIndentTarget? new IndentedSourcePartEdit(target, count) : target;
    }
}