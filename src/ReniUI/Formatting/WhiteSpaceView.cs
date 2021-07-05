using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;

namespace ReniUI.Formatting
{
    /// <summary>
    ///     Encapsulates all comment, line-break and space formatting for a token.
    ///     This class has a very high complexity since the target is quite complex.
    ///     It is mainly to ensure smooth behaviour of source editor where the formatting is made for.
    ///     The member names by default belong to thing on the left side of the token.
    ///     Things on the right side contain this fact in their name.
    /// </summary>
    sealed class WhiteSpaceView : DumpableObject, ISourcePartEdit, IEditPieces
    {
        sealed class CacheContainer
        {
            public IEnumerable<IItem>[] CommentGroups;
            public SourcePart[] LineBreakGroups;
            public SourcePart Spaces;
        }

        readonly CacheContainer Cache = new();
        readonly Configuration Configuration;

        readonly bool IsSeparatorRequired;
        readonly int MinimalLineBreakCount;

        readonly IEnumerable<IItem> Target;

        internal WhiteSpaceView
        (
            IEnumerable<IItem> target
            , Configuration configuration
            , bool isSeparatorRequired
            , int minimalLineBreakCount = 0
        )
        {
            (target != null).Assert();
            target.Any().Assert();
            Target = target;
            IsSeparatorRequired = isSeparatorRequired;
            MinimalLineBreakCount = minimalLineBreakCount;
            Configuration = configuration;
            (!(CommentGroups.Any() && IsSeparatorRequired)).Assert();
            StopByObjectIds();
        }

        /// <summary>
        ///     Edits, i. e. pairs of old text/new text are generated to accomplish the target text.
        ///     The goal is, to change only things necessary to allow editors to work smoothly
        /// </summary>
        /// <returns></returns>
        IEnumerable<Edit> IEditPieces.Get(EditPieceParameter parameter)
            => GetLineBreakEdits()
                .Concat(GetSpaceEdits(parameter.IndentCharacterCount));

        ISourcePartEdit ISourcePartEdit.AddLineBreaks(int count)
        {
            (count > 0).Assert();
            return new WhiteSpaceView(Target, Configuration, IsSeparatorRequired
                , T(MinimalLineBreakCount, count).Max());
        }

        bool ISourcePartEdit.HasLines
            => Target.HasLineComment() || GetTargetLineBreakCount(MinimalLineBreakCount) > 0;

        ISourcePartEdit ISourcePartEdit.Indent(int count) => this.CreateIndent(count);

        SourcePart ISourcePartEdit.SourcePart => Target.SourcePart();

        protected override string GetNodeDump() =>
            Target.SourcePart().GetDumpAroundCurrent(10) + " " + base.GetNodeDump();

        IEnumerable<IItem>[] CommentGroups
            => Cache.CommentGroups ?? (Cache.CommentGroups = GetCommentGroups());

        SourcePart Spaces
            => Cache.Spaces ?? (Cache.Spaces = GetSpaces());

        int LineBreakCount => LineBreakGroups.Length;

        SourcePart LineBreaksAnchor
            => (LineBreakGroups.FirstOrDefault() ?? Spaces).Start.Span(0);

        SourcePart[] LineBreakGroups
            => Cache.LineBreakGroups ?? (Cache.LineBreakGroups = GetLineBreakGroups());

        /// <summary>
        ///     Get edits to ensure the correct number of line breaks.
        ///     Extra line breaks are added at first.
        ///     Then current line breaks are re-used from left to right.
        ///     For those line breaks edits might be generated if they had leading spaces.
        ///     Line breaks that are not used anymore are removed.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Edit> GetLineBreakEdits()
        {
            var delta = GetTargetLineBreakCount(MinimalLineBreakCount) - LineBreakCount;
            if(delta > 0)
                yield return Edit.Create("+LineBreaks", LineBreaksAnchor, "\n".Repeat(delta));

            for(var index = 0; index < LineBreakCount; index++)
            {
                var groupPart = LineBreakGroups[index];

                if(index >= GetTargetLineBreakCount(MinimalLineBreakCount))
                    yield return Edit.Create("-AllLineBreaks", groupPart); // Remove spaces and line break
                else if(groupPart.Length > 1)
                    yield return Edit.Create("-SomeLineBreaks", groupPart.Start.Span(groupPart.Length - 1));
                // remove spaces, keep line break
                // else it is only a line break, that can be kept
            }
        }

        IEnumerable<Edit> GetSpaceEdits(int indentCharacterCount)
        {
            (GetTargetLineBreakCount(MinimalLineBreakCount) == 0 || !GetTargetSeparator(MinimalLineBreakCount)).Assert
                ();
            Spaces.Id.All(c => c == ' ').Assert();

            var targetSpacesCount
                = GetTargetLineBreakCount(MinimalLineBreakCount) != 0
                    ? indentCharacterCount
                    : GetTargetSeparator(MinimalLineBreakCount)
                        ? 1
                        : 0;

            var delta = targetSpacesCount - Spaces.Length;
            if(delta == 0)
                yield break;

            var deletedPart = Spaces.Start.Span(T(-delta, 0).Max());
            var newText = " ".Repeat(T(delta, 0).Max());
            yield return Edit.Create("+-Spaces", deletedPart, newText);
        }

        int GetTargetLineBreakCount(int minimalLineBreakCount)
        {
            if(minimalLineBreakCount >= LineBreakCount)
                return minimalLineBreakCount;

            var limit = Configuration.EmptyLineLimit;
            if(limit == null)
                return LineBreakCount;

            var keepLineBreaks = T(LineBreakCount, limit.Value).Min();
            return T(minimalLineBreakCount, keepLineBreaks).Max();
        }

        bool GetTargetSeparator(int minimalLineBreakCount)
            => GetTargetLineBreakCount(minimalLineBreakCount) == 0 && IsSeparatorRequired;

        /// <summary>
        ///     If also comments or line breaks are existent
        ///     only the spaces following the last comment or line break are returned.
        /// </summary>
        /// <returns></returns>
        SourcePart GetSpaces()
        {
            Target.Any().Assert();

            var last = Target.Last();
            if(!last.IsSpace())
                return last.SourcePart.End.Span(0);

            return Target
                .Split(item => !item.IsSpace())
                .Last()
                .SourcePart();
        }

        /// <summary>
        ///     If also comments are existent
        ///     only the line breaks following the last comment are returned.
        ///     The line-break-preceding spaces are included in result.
        ///     Spaces behind the last line break are not returned.
        /// </summary>
        /// <returns></returns>
        SourcePart[] GetLineBreakGroups()
        {
            var lastItem = Target.Last();
            if(lastItem.IsComment())
                return new SourcePart[0];

            var lastLineBreakGroup = Target
                .Split(item => item.IsComment())
                .Last();

            return lastLineBreakGroup
                .Split(item => item.IsLineEnd(), false)
                .Where(group => group.Last().IsLineEnd())
                .Select(group => group.SourcePart())
                .ToArray();
        }

        IEnumerable<IItem>[] GetCommentGroups()
            => Target
                .Split(item => item.IsComment(), false)
                .Where(group => group.Last().IsComment())
                .ToArray();
    }
}