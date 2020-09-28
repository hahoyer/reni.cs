using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace ReniUI.Formatting
{
    sealed class EmptyWhiteSpaceView : DumpableObject, ISourcePartEdit, IEditPieces
    {
        readonly SourcePosn Anchor;
        readonly bool IsSeparatorRequired;

        internal EmptyWhiteSpaceView
        (
            SourcePosn anchor,
            bool isSeparatorRequired)
        {
            Anchor = anchor;
            IsSeparatorRequired = isSeparatorRequired;
        }

        IEnumerable<Edit> IEditPieces.Get(EditPieceParameter parameter)
        {
            var spaceCount
                = parameter.LineBreakCount > 0 ? parameter.IndentCharacterCount :
                IsSeparatorRequired ? 1 : 0;
            var newText = "\n".Repeat(parameter.LineBreakCount) + " ".Repeat(spaceCount);

            if(newText != "")
                yield return Edit.Create("+LineBreaksSpaces", Anchor.Span(0), newText);
        }

        protected override string GetNodeDump() => Anchor.DebuggerDumpString + " " + base.GetNodeDump();
        bool ISourcePartEdit.HasLines => false;
    }
}