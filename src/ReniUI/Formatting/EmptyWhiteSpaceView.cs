using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni;

namespace ReniUI.Formatting
{
    sealed class EmptyWhiteSpaceView : DumpableObject, ISourcePartEdit, IEditPieces
    {
        readonly bool IsSeparatorRequired;

        [EnableDump]
        [EnableDumpExcept(0)]
        readonly int MinimalLineBreakCount;

        readonly SourcePosn Anchor;

        internal EmptyWhiteSpaceView
        (
            SourcePosn anchor,
            bool isSeparatorRequired,
            int minimalLineBreakCount)
        {
            Anchor = anchor;
            IsSeparatorRequired = isSeparatorRequired;
            MinimalLineBreakCount = minimalLineBreakCount;
            StopByObjectIds(235);
        }

        IEnumerable<Edit> IEditPieces.Get(EditPieceParameter parameter)
        {
            var spaceCount
                = MinimalLineBreakCount > 0 ? parameter.IndentCharacterCount :
                IsSeparatorRequired ? 1 : 0;
            var newText = "\n".Repeat(MinimalLineBreakCount) + " ".Repeat(spaceCount);

            if(newText != "")
                yield return Edit.Create("+LineBreaksSpaces", Anchor.Span(0), newText);
        }

        protected override string GetNodeDump() => Anchor.DebuggerDumpString + " " + base.GetNodeDump();
    }
}