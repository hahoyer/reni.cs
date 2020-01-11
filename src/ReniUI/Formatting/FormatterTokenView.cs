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
    /// </summary>
    sealed class FormatterTokenView : DumpableObject, ISourcePartEdit, IEditPieces
    {
        sealed class CacheContainer
        {
            public IEnumerable<IItem>[] CommentGroups;
            public SourcePart[] LineBreaksBefore;
            public SourcePart SpacesBefore;
        }

        readonly CacheContainer Cache = new CacheContainer();


        [EnableDump]
        readonly ISeparatorType LeftSeparator;

        [EnableDump]
        readonly int MinimalLineBreaksBefore;

        [EnableDump]
        readonly IToken Token;

        [EnableDump]
        readonly int LineBreaksAfter;

        readonly Configuration Configuration;

        internal FormatterTokenView
        (
            ISeparatorType leftSeparator,
            int lineBreaksBefore,
            IToken token,
            int lineBreaksAfter,
            Configuration configuration)
        {
            LeftSeparator = leftSeparator;
            MinimalLineBreaksBefore = lineBreaksBefore;
            Token = token;
            LineBreaksAfter = lineBreaksAfter;
            Configuration = configuration;
        }

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

            if(LineBreaksAfter > 0)
            {
                NotImplementedMethod(parameter);
                return default;
            }

            return GetLineBreakEditsBefore().Concat(GetSpaceEditsBefore(parameter.Indent));
        }

        IEnumerable<Edit> GetLineBreakEditsBefore()
        {
            var delta = NewLineBreaksBefore - LineBreakGroupsBefore.Length;
            if(delta > 0)
                yield return Edit.Create(LineBreaksAnchor, "\n".Repeat(delta));

            for(var index = 0; index < LineBreakGroupsBefore.Length; index++)
            {
                var groupPart = LineBreakGroupsBefore[index];

                if(index >= NewLineBreaksBefore)
                    yield return Edit.Create(groupPart); // Remove spaces and break
                else if(groupPart.Length > 1)
                    yield return Edit.Create(groupPart.Start.Span(groupPart.Length - 1));
                // remove spaces, keep break
                // else it is only a line break, that can be kept
            }
        }

        IEnumerable<Edit> GetSpaceEditsBefore(int indent)
        {
            var newSpaces = NewLeftSeparator.Text.Length;
            if(NewLineBreaksBefore > 0)
                newSpaces += indent * Configuration.IndentCount;

            Tracer.Assert(SpacesBefore.Id.All(c => c == ' '));

            var delta = newSpaces - SpacesBefore.Length;
            if(delta == 0)
                yield break;

            var deletedPart = SpacesBefore.Start.Span(T(-delta, 0).Max());
            var newText = " ".Repeat(T(delta, 0).Max());
            yield return Edit.Create(deletedPart, newText);
        }

        IEnumerable<IItem>[] CommentGroups
            => Cache.CommentGroups ?? (Cache.CommentGroups = GetCommentGroups());

        SourcePart SpacesBefore
            => Cache.SpacesBefore ?? (Cache.SpacesBefore = GetSpacesBefore());

        int NewLineBreaksBefore
        {
            get
            {
                var emptyLineLimit = Configuration.EmptyLineLimit ?? LineBreakGroupsBefore.Length;
                var keepLineBreaksBefore = T(LineBreakGroupsBefore.Length, emptyLineLimit).Min();
                return T(MinimalLineBreaksBefore, keepLineBreaksBefore).Max();
            }
        }

        ISeparatorType NewLeftSeparator
            => NewLineBreaksBefore == 0
                ? LeftSeparator
                : SeparatorType.ContactSeparator;

        SourcePart LineBreaksAnchor 
            => (LineBreakGroupsBefore.FirstOrDefault() ?? SpacesBefore).Start.Span(length: 0);

        SourcePart[] LineBreakGroupsBefore
            => Cache.LineBreaksBefore ?? (Cache.LineBreaksBefore = GetLineBreaksBefore());

        SourcePart GetSpacesBefore()
        {
            var result = Token
                .PrecededWith
                .Split(item => !item.IsWhiteSpace(), assignSeparatorAtTopOfList: false)
                .LastOrDefault()
                ?
                .ToArray();
            return result?.Last().IsWhiteSpace() == true ? result.SourcePart() : Token.Characters.Start.Span(length: 0);
        }

        SourcePart[] GetLineBreaksBefore()
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