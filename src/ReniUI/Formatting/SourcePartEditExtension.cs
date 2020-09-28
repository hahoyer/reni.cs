using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

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

        internal static readonly ISourcePartEdit MinimalLineBreak = new SpecialEdit(id: "MinimalLineBreak");
        internal static readonly ISourcePartEdit MinimalLineBreaks = new SpecialEdit(id: "MinimalLineBreaks");
        internal static readonly ISourcePartEdit EnsureSeparator = new SpecialEdit(id: "EnsureSeparator");
        internal static readonly ISourcePartEdit ToRight = new SpecialEdit(id: "ToRight");
        internal static readonly ISourcePartEdit ToLeft = new SpecialEdit(id: "ToLeft");
        internal static readonly ISourcePartEdit EndOfFile = new SpecialEdit(id: "EndOfFile");

        internal static IEnumerable<Edit> 
            GetEditPieces(this IEnumerable<ISourcePartEdit> target, Configuration configuration)
        {
            var currentPosition = 0;
            var currentIndex = 0;

            var parameter = new EditPieceParameter(configuration);

            var result = new List<Edit>();
            foreach(var part in target)
            {
                if(part == ToRight)
                    parameter.Indent++;
                else if(part == ToLeft)
                    --parameter.Indent;
                else if(part == MinimalLineBreak)
                {
                    parameter.LineBreakCount = T(parameter.LineBreakCount, 1).Max();
                    parameter.IsSeparatorRequired = false;
                }
                else if(part == MinimalLineBreaks)
                {
                    parameter.LineBreakCount = T(parameter.LineBreakCount, 2).Max();
                    parameter.IsSeparatorRequired = false;
                }
                else if(part == EnsureSeparator)
                    parameter.IsSeparatorRequired = true;
                else if(part == EndOfFile)
                    parameter.IsEndOfFile = true;
                else if(part is IEditPieces spe)
                {
                    var edits = spe.Get(parameter).ToArray();
                    foreach(var edit in edits)
                    {
                        Tracer.Assert(currentPosition <= edit.Location.Position);
                        currentPosition = edit.Location.EndPosition;
                        result.Add(edit);
                    }

                    parameter.Reset();
                }
                else
                    Dumpable.NotImplementedFunction(target.ToArray(), configuration);

                currentIndex++;
            }

            return result;
        }

        internal static ISourcePartEdit AsMinimalLineBreaks(this int target)
        {
            switch(target)
            {
                case 1: return MinimalLineBreak;
                case 2: return MinimalLineBreaks;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target,"Invalid value for minimal line break setup. Must be 1 or 2.");
            }
        }

        static IEnumerable<ISourcePartEdit> IndentRight(this IEnumerable<ISourcePartEdit> target)
            => new[] {ToRight}.Concat(target).Concat(new[] {ToLeft});

        static IEnumerable<ISourcePartEdit> IndentLeft(this IEnumerable<ISourcePartEdit> target)
            => new[] {ToLeft}.Concat(target).Concat(new[] {ToRight});

        internal static IEnumerable<ISourcePartEdit> 
            Indent(this IEnumerable<ISourcePartEdit> target, IndentDirection direction)
        {
            switch(direction)
            {
                case IndentDirection.ToLeft: return IndentLeft(target);
                case IndentDirection.ToRight: return IndentRight(target);
                default: return target;
            }
        }

        static IEnumerable<TValue> T<TValue>(params TValue[] value) => value;
    }

    interface IEditPieces
    {
        IEnumerable<Edit> Get(EditPieceParameter parameter);
    }

    enum IndentDirection
    {
        ToLeft,
        ToRight,
        NoIndent
    }
}