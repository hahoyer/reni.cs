using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class FormatterToken : DumpableObject
    {
        internal static IEnumerable<FormatterToken> Create(Syntax syntax) => Create(syntax.Token);

        static IEnumerable<FormatterToken> Create(IToken token)
        {
            var anchor = token.SourcePart().Start;
            var items = token.PrecededWith.ToArray();
            var lineBreaks = new List<int>();
            var spaces = new List<int>();

            foreach(var item in items)
                if(item.IsLineBreak())
                {
                    spaces = new List<int>();
                    lineBreaks.Insert(0, item.SourcePart.Position);
                    anchor = item.SourcePart.End;
                }
                else if(item.IsComment())
                {
                    yield return new FormatterToken(lineBreaks, anchor, spaces);
                    spaces = new List<int>();
                    lineBreaks = new List<int>();
                }
                else
                    spaces.Add(item.SourcePart.EndPosition);

            yield return new FormatterToken(lineBreaks, anchor, spaces);
        }

        readonly SourcePosn Anchor;

        readonly int[] LineBreaks;
        readonly int[] Spaces;

        FormatterToken
        (
            IEnumerable<int> lineBreaks,
            SourcePosn anchor,
            IEnumerable<int> spaces)
        {
            LineBreaks = lineBreaks.ToArray();
            Anchor = anchor;
            Spaces = spaces.ToArray();
        }

        [EnableDumpExcept(0)]
        int LineBreakCount => LineBreaks.Length;

        [EnableDumpExcept(0)]
        int SpaceCount => Spaces.Length;

        internal SourcePartEdit ToSourcePartEdit() => new SourcePartEdit(this);

        internal Edit GetEditPiece(EditPieceParameter parameter)
            => parameter.GetEditPiece(LineBreaks, Anchor, Spaces);
    }
}