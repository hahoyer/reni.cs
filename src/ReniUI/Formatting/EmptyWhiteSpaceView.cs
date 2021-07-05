using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace ReniUI.Formatting
{
    sealed class EmptyWhiteSpaceView : DumpableObject, ISourcePartEdit, IEditPieces
    {
        readonly SourcePart Anchor;
        readonly bool IsSeparatorRequired;
        readonly int MinimalLineBreakCount;

        internal EmptyWhiteSpaceView
        (
            SourcePart anchor
            , bool isSeparatorRequired
            , int minimalLineBreakCount = 0
        )
        {
            Anchor = anchor;
            IsSeparatorRequired = isSeparatorRequired;
            MinimalLineBreakCount = minimalLineBreakCount;
            //StopByObjectIds(276);
        }

        IEnumerable<Edit> IEditPieces.Get(EditPieceParameter parameter)
        {
            var spaceCount
                = MinimalLineBreakCount > 0
                    ? parameter.IndentCharacterCount
                    : IsSeparatorRequired
                        ? 1
                        : 0;
            var newText = "\n".Repeat(MinimalLineBreakCount) + " ".Repeat(spaceCount);

            if(newText != "")
                yield return Edit.Create("+LineBreaksSpaces", Anchor.Start.Span(0), newText);
        }

        ISourcePartEdit ISourcePartEdit.AddLineBreaks(int count)
            => new EmptyWhiteSpaceView(Anchor, IsSeparatorRequired, T(MinimalLineBreakCount, count).Max());

        bool ISourcePartEdit.HasLines => MinimalLineBreakCount > 0;
        ISourcePartEdit ISourcePartEdit.Indent(int count) => this.CreateIndent(count);
        SourcePart ISourcePartEdit.SourcePart => Anchor;

        protected override string GetNodeDump() => Anchor.GetDumpAroundCurrent(5) + " " + base.GetNodeDump();
    }
}