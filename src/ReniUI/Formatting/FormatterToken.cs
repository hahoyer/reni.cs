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
        public static IEnumerable<FormatterToken> Create(Syntax syntax)
        {
            var token = syntax.Token;
            var start = token.SourcePart().Start;
            var items = token.PrecededWith.ToArray();
            var lineCount = 0;
            var spaceCount = 0;

            foreach(var item in items)
                if(item.IsLineBreak())
                {
                    spaceCount = 0;
                    lineCount++;
                }
                else if(item.IsComment())
                {
                    var end = item.SourcePart.End;
                    var span = start.Span(end);
                    yield return new FormatterToken(lineCount, spaceCount, span, item, null);
                    start = end;
                }
                else
                    spaceCount++;

            var sourcePart = start.Span(token.Characters.End);
            var tokenClass = syntax.TokenClass;
            yield return new FormatterToken(lineCount, spaceCount, sourcePart, null, tokenClass);
        }

        [EnableDumpExcept(null)]
        internal readonly IItem Item;

        [EnableDumpExcept(0)]
        internal readonly int LineBreakCount;

        internal readonly SourcePart SourcePart;

        [EnableDumpExcept(0)]
        internal readonly int SpaceCount;

        [EnableDumpExcept(null)]
        internal readonly ITokenClass Token;

        FormatterToken(int lineBreakCount, int spaceCount, SourcePart sourcePart, IItem item, ITokenClass token)
        {
            LineBreakCount = lineBreakCount;
            SpaceCount = spaceCount;
            SourcePart = sourcePart;
            Item = item;
            Token = token;
        }

        [DisableDump]
        internal bool HasLineBreak => LineBreakCount > 0 || Item != null && Item.HasLines();

        internal SourcePartEdit ToSourcePartEdit() => new SourcePartEdit(this);

        internal bool LineBreakScan(ref int? lineLength)
        {
            if(HasLineBreak)
                return true;
            if(lineLength == null)
                return false;
            lineLength -= SourcePart.Length;
            return lineLength <= 0;
        }

        internal IEnumerable<Edit> GetEditPieces(EditPieceParameter parameter)
            => parameter.GetEditPieces(SourcePart.Start, LineBreakCount, SpaceCount);
    }
}