using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace ReniUI.Formatting
{
    static class SourcePartEditExtension
    {
        [Obsolete("", true)]
        sealed class SpecialEdit : DumpableObject, ISourcePartEdit
        {
            readonly bool HasLines;
            readonly string Id;

            public SpecialEdit(string id, bool hasLines = false)
            {
                Id = id;
                HasLines = hasLines;
            }

            bool ISourcePartEdit.HasLines => HasLines;
            ISourcePartEdit ISourcePartEdit.Indent(int count) => this.CreateIndent(count);

            SourcePart ISourcePartEdit.SourcePart => throw new NotImplementedException();

            public override string ToString() => Id;
            protected override string GetNodeDump() => Id;
        }

        [Obsolete("", true)]
        internal static readonly ISourcePartEdit MinimalLineBreak
            = new SpecialEdit("MinimalLineBreak", true);

        [Obsolete("", true)]
        internal static readonly ISourcePartEdit MinimalLineBreaks
            = new SpecialEdit("MinimalLineBreaks", true);

        [Obsolete("", true)]
        internal static readonly ISourcePartEdit EnsureSeparator = new SpecialEdit("EnsureSeparator");

        [Obsolete("", true)]
        internal static readonly ISourcePartEdit ToRight = new SpecialEdit("ToRight");

        [Obsolete("", true)]
        internal static readonly ISourcePartEdit ToLeft = new SpecialEdit("ToLeft");

        [Obsolete("", true)]
        internal static readonly ISourcePartEdit EndOfFile = new SpecialEdit("EndOfFile");

        internal static IEnumerable<Edit>
            GetEditPieces(this IEnumerable<ISourcePartEdit> target, Configuration configuration)
        {
            var parameter = new EditPieceParameter(configuration);

            return target
                .OrderBy(node => node.SourcePart.Position)
                .ThenBy(node => node.SourcePart.Length)
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

        static IEnumerable<TValue> T<TValue>(params TValue[] value) => value;

        public static ISourcePartEdit CreateIndent(this ISourcePartEdit target, int count)
            => count != 0 && target.HasLines? new IndentedSourcePartEdit(target, count) : target;

        [Obsolete("", true)]
        public static IEnumerable<ISourcePartEdit> Indent
            (this IEnumerable<ISourcePartEdit> target, IndentDirection toRight) => throw new NotImplementedException();
    }

    class IndentedSourcePartEdit : DumpableObject, ISourcePartEdit, IEditPieces
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