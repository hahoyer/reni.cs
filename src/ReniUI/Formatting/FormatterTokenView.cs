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
    ///     This class has a very high complexity, since the target is quite complex.
    ///     The member names by default belong to thing on the left side of the token.
    ///     Things on the right side contain this fact in their name.
    /// </summary>
    sealed class FormatterTokenView : DumpableObject, ISourcePartEdit, IEditPieces
    {
        sealed class CacheContainer
        {
            public IEnumerable<IItem>[] CommentGroups;
            public SourcePart[] LineBreaks;
            public SourcePart Spaces;
        }

        readonly CacheContainer Cache = new CacheContainer();

        readonly ISeparatorType Separator;

        [EnableDump]
        [EnableDumpExcept(exception: 0)]
        readonly int LineBreaksFromNeighbor;

        [EnableDump]
        [EnableDumpExcept(exception: 0)]
        readonly int MinimalLineBreaks;

        readonly IToken Token;

        readonly Configuration Configuration;

        internal FormatterTokenView
        (
            ISeparatorType separator,
            int lineBreaksFromNeighbor,
            int lineBreaks,
            IToken token,
            Configuration configuration
        )
        {
            Separator = separator;

            Tracer.Assert(lineBreaksFromNeighbor == 0 || lineBreaks == 0);
            LineBreaksFromNeighbor = lineBreaksFromNeighbor;
            MinimalLineBreaks = lineBreaks + lineBreaksFromNeighbor;
            Token = token;
            Configuration = configuration;
        }

        [EnableDump]
        [EnableDumpExcept(exception: "")]
        string TokenDump => Token.Characters.Id;

        [EnableDump]
        [EnableDumpExcept(exception: "")]
        string TargetSeparatorDump => TargetSeparator.Text;

        /// <summary>
        ///     Edits, i. e. pairs of oldtext/newtext are generated to accomplish the target text.
        ///     The goal is, to change only things necessary to allow editors to work smoothly
        /// </summary>
        /// <returns></returns>
        IEnumerable<Edit> IEditPieces.Get(EditPieceParameter parameter)
        {
            if(Token == null)
            {
                NotImplementedMethod(parameter);
                return default;
            }

            if(CommentGroups.Any())
            {
                NotImplementedMethod(parameter);
                return default;
            }

            return GetLineBreakEdits().Concat(GetSpaceEdits(parameter.IndentCharacterCount));
        }

        /// <summary>
        ///     Extra line breaks are added at first.
        ///     Then current line breaks are re-used from left to right.
        ///     For those edits could be generated if they had leading spaces.
        ///     Line breaks that are not used anymore are removed.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Edit> GetLineBreakEdits()
        {
            var delta = TargetLineBreaks - LineBreakGroups.Length;
            if(delta > 0)
                yield return Edit.Create(LineBreaksAnchor, "\n".Repeat(delta));

            for(var index = 0; index < LineBreakGroups.Length; index++)
            {
                var groupPart = LineBreakGroups[index];

                if(index >= TargetLineBreaks)
                    yield return Edit.Create(groupPart); // Remove spaces and break
                else if(groupPart.Length > 1)
                    yield return Edit.Create(groupPart.Start.Span(groupPart.Length - 1));
                // remove spaces, keep break
                // else it is only a line break, that can be kept
            }
        }

        IEnumerable<Edit> GetSpaceEdits(int indentCharacterCount)
        {
            Tracer.Assert(TargetLineBreaks == 0 || TargetSeparator == SeparatorType.ContactSeparator);
            Tracer.Assert(Spaces.Id.All(c => c == ' '));
            Tracer.Assert(TargetSeparator.Text.All(c => c == ' '));

            var targetSpacesCount = TargetLineBreaks == 0
                ? TargetSeparator.Text.Length
                : indentCharacterCount;

            var delta = targetSpacesCount - Spaces.Length;
            if(delta == 0)
                yield break;

            var deletedPart = Spaces.Start.Span(T(-delta, 0).Max());
            var newText = " ".Repeat(T(delta, 0).Max());
            yield return Edit.Create(deletedPart, newText);
        }

        IEnumerable<IItem>[] CommentGroups
            => Cache.CommentGroups ?? (Cache.CommentGroups = GetCommentGroups());

        SourcePart Spaces
            => Cache.Spaces ?? (Cache.Spaces = GetSpaces());

        /// <summary>
        ///     Can be controlled by configuration value EmptyLineLimit.
        ///     If not set, all line breaks are retained.
        ///     However, new line break can be added if required
        /// </summary>
        [EnableDump]
        [EnableDumpExcept(exception: 0)]
        int TargetLineBreaks
        {
            get
            {
                var emptyLineLimit = Configuration.EmptyLineLimit ?? LineBreakGroups.Length;
                var keepLineBreaksBefore = T(LineBreakGroups.Length, emptyLineLimit).Min();
                return T(MinimalLineBreaks, keepLineBreaksBefore).Max();
            }
        }

        ISeparatorType TargetSeparator
            => TargetLineBreaks == 0
                ? Separator
                : SeparatorType.ContactSeparator;

        SourcePart LineBreaksAnchor
            => (LineBreakGroups.FirstOrDefault() ?? Spaces).Start.Span(length: 0);

        SourcePart[] LineBreakGroups
            => Cache.LineBreaks ?? (Cache.LineBreaks = GetLineBreaks());

        SourcePart GetSpaces()
        {
            var result = Token
                .PrecededWith
                .Split(item => !item.IsWhiteSpace(), assignSeparatorAtTopOfList: false)
                .LastOrDefault()
                ?
                .ToArray();
            return result?.Last().IsWhiteSpace() == true ? result.SourcePart() : Token.Characters.Start.Span(length: 0);
        }

        SourcePart[] GetLineBreaks()
        {
            var result = Token
                .PrecededWith
                .Split(item => item.IsComment(), assignSeparatorAtTopOfList: false)
                .LastOrDefault()
                ?
                .ToArray();
            if(result?.Last().IsNonComment() == true)
                return result
                    .Split(item => item.IsLineBreak(), assignSeparatorAtTopOfList: false)
                    .Where(group => group.Last().IsLineBreak())
                    .Select(group => group.SourcePart())
                    .ToArray();

            return new SourcePart[0];
        }

        IEnumerable<IItem>[] GetCommentGroups()
            => Token
                .PrecededWith
                .Split(item => item.IsComment(), assignSeparatorAtTopOfList: false)
                .Where(group => group.Last().IsComment())
                .ToArray();

        static IEnumerable<TValue> T<TValue>(params TValue[] value) => value;
    }
}