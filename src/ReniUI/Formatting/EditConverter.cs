using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Scanner;

namespace ReniUI.Formatting
{
    sealed class EditConverter
    {
        static IEnumerable<TValue> T<TValue>(params TValue[] value) => value;

        readonly ContextForConvertToEdits Context;

        readonly SourcePosn Anchor;
        readonly SourcePart[] LineBreakPrefixes;
        readonly int[] RelativeSpacePositions;
        readonly bool HasCommentLineBreak;
        int? NewLineBreakCountCache;

        int IndentCharacterCount
            => Context.Indent > 0 ? Context.Indent * Context.Configuration.IndentCount : 0;

        public EditConverter
        (
            ContextForConvertToEdits context,
            SourcePart[] lineBreakPrefixes,
            SourcePosn anchor,
            int[] relativeSpacePositions,
            bool hasCommentLineBreak)
        {
            Context = context;
            LineBreakPrefixes = lineBreakPrefixes;
            Anchor = anchor;
            RelativeSpacePositions = relativeSpacePositions;
            HasCommentLineBreak = hasCommentLineBreak;
        }

        public IEnumerable<Edit> Value => NoEditsRequired ? Enumerable.Empty<Edit>() : RealValue;


        public IEnumerable<Edit> RealValue
        {
            get
            {
                var lineBreakDelta = NewLineBreakCount - LineBreakPrefixes.Length;
                var lineBreakStartPosition = lineBreakDelta < 0
                    ? LineBreakPrefixes[NewLineBreakCount].End - Anchor
                    : 0;
                var lineBreakText = "\n".Repeat(lineBreakDelta);

                var isSeparatorRequired = NewLineBreakCount == 0 && Context.IsSeparatorRequired;
                var newSpacesCount = isSeparatorRequired ? 1 : 0;
                if(Context.LineBreakCount > 0)
                    newSpacesCount += IndentCharacterCount;

                var spacesDelta = newSpacesCount - RelativeSpacePositions.Length;
                var spacesText = spacesDelta > 0 ? " ".Repeat(spacesDelta) : "";
                var spacesEndPosition = spacesDelta < 0 ? RelativeSpacePositions[-spacesDelta - 1] : 0;

                var location = (Anchor + lineBreakStartPosition).Span(Anchor + spacesEndPosition);
                var newText = lineBreakText + spacesText;

                var textEdit = new Edit {Location = location, NewText = newText};

                return LineBreakPrefixes
                    .Where(item => item.Length > 0 && item.End <= location.Start)
                    .Select(item => new Edit {Location = item, NewText = ""})
                    .Concat(T(textEdit));
            }
        }

        bool NoEditsRequired
            => NewLineBreakCount == LineBreakPrefixes.Length || 
               NewSpacesCount <= RelativeSpacePositions.Length || 
               NewLineBreakCount < LineBreakPrefixes.Length &&
               LineBreakPrefixes[NewLineBreakCount].End.Position == Anchor.Position;

        int NewSpacesCount
            => (NewLineBreakCount == 0 && Context.IsSeparatorRequired ? 1 : 0) +
               (Context.LineBreakCount > 0 ? IndentCharacterCount : 0);

        int NewLineBreakCount => (NewLineBreakCountCache ?? (NewLineBreakCountCache = GetNewLineBreakCount())).Value;

        int GetNewLineBreakCount()
        {
            if(Context.IsEndOfFile && Context.Configuration.LineBreakAtEndOfText != null)
                return Context.Configuration.LineBreakAtEndOfText.Value ? 1 : 0;

            var effectiveLineBreakCount = LineBreakPrefixes.Length;

            if(Context.Configuration.EmptyLineLimit != null &&
               effectiveLineBreakCount > Context.Configuration.EmptyLineLimit.Value)
                effectiveLineBreakCount = Context.Configuration.EmptyLineLimit.Value;

            var result = Context.LineBreakCount - (HasCommentLineBreak ? 1 : 0);

            return result > effectiveLineBreakCount ? result : effectiveLineBreakCount;
        }
    }
}