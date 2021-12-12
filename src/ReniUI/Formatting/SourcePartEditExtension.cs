using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using JetBrains.Annotations;

namespace ReniUI.Formatting
{
    static class SourcePartEditExtension
    {
        internal static IEnumerable<Edit>
            GetEditPieces(this IEnumerable<ISourcePartEdit> target, Configuration configuration)
        {
            var parameter = new EditPieceParameter(configuration);

            var sortedTargets = target
                .OrderBy(node => node.SourcePart.Position)
                .ThenBy(node => node.SourcePart.Length)
                .ToArray();
            return sortedTargets
                .SelectMany(part => part.GetEditPieces(parameter))
                .ToArray();
        }

        internal static IEnumerable<Edit> GetEditPieces(this ISourcePartEdit target, EditPieceParameter parameter)
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
            => count != 0 && target.HasLines? new IndentedSourcePartEdit(target, count) : target;
    }

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

        IEnumerable<Edit> IEditPieces.Get(EditPieceParameter parameter)
        {
            var currentIndent = parameter.Indent;
            parameter.Indent += Count;
            var result = Target.GetEditPieces(parameter);
            parameter.Indent = currentIndent;
            return result;
        }

        bool ISourcePartEdit.HasLines => Target.HasLines;

        ISourcePartEdit ISourcePartEdit.Indent(int count)
        {
            var newCount = count + Count;
            return newCount == 0? Target : new IndentedSourcePartEdit(Target, newCount);
        }

        SourcePart ISourcePartEdit.SourcePart => Target.SourcePart;
    }

    interface IEditPieces
    {
        IEnumerable<Edit> Get(EditPieceParameter parameter);
    }

    [Obsolete("", true)]
    enum IndentDirection
    {
        ToLeft
        , ToRight
        , NoIndent
    }
}