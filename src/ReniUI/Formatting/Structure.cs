using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class Structure : DumpableObject, IStructure
    {
        readonly Context Context;

        [EnableDump]
        readonly Formatter Formatter;
        readonly Syntax Syntax;

        bool? IsLineSplitRequiredCache;

        internal Structure(Syntax syntax, Context context)
        {
            Formatter = Formatter.CreateFormatter(syntax);
            Syntax = syntax;
            Context = context;
            Tracer.Assert(Syntax != null);
        }

        Syntax IStructure.Syntax => Syntax;

        (IEnumerable<ISourcePartEdit>, int) IStructure.Get(int minimalLineBreaks)
        {
            var trace = Formatter.IsTrace;
            StartMethodDump(trace, minimalLineBreaks);
            try
            {
                Tracer.ConditionalBreak(trace);
                var result = new List<ISourcePartEdit>();
                if(Syntax.Left != null)
                {
                    var leftOfLeft = Formatter.LineBreaksLeftOfLeft;
                    if(minimalLineBreaks < leftOfLeft)
                        minimalLineBreaks = leftOfLeft;
                    var leftSide = GetLeftSide(minimalLineBreaks);
                    result.AddRange(leftSide.edits.Indent(Formatter.IndentLeftSide));
                    minimalLineBreaks = leftSide.lineBreaks;
                }

                result.AddRange(GetTokenEdits(minimalLineBreaks).Indent(Formatter.IndentToken));
                var lineBreaksOnRightSide = LineBreaksOnRightSide;
                if(Syntax.Right != null)
                {
                    var rightSide = GetRightSide(lineBreaksOnRightSide);
                    result.AddRange(rightSide.edits.Indent(Formatter.IndentRightSide));
                    lineBreaksOnRightSide = T(rightSide.lineBreaks, Formatter.LineBreaksRightOfRight).Max();
                }

                var b = AsString(result.ToArray());
                return ReturnMethodDump((result.ToArray(), lineBreaksOnRightSide), false);
            }
            finally
            {
                EndMethodDump();
            }
        }

        IEnumerable<ISourcePartEdit> GetTokenEdits(int minimalLineBreaks)
        {
            yield return new FormatterTokenView
            (
                Syntax.LeftSideSeparator(),
                minimalLineBreaks,
                LineBreaksOnLeftSide,
                Syntax.Token,
                Context.Configuration
            );
        }

        string AsString(ISourcePartEdit[] target)
            => target
                .GetEditPieces(Context.Configuration)
                .Combine(Syntax.SourcePart.Source.All);

        [EnableDump]
        string FlatResult => Syntax.FlatFormat(Context.Configuration.EmptyLineLimit);

        int LineBreaksOnLeftSide
            => IsLineSplitRequired ? Formatter.LineBreaksBeforeToken(Context) : 0;

        int LineBreaksOnRightSide
            => IsLineSplitRequired ? Formatter.LineBreaksAfterToken(Context) : 0;

        [EnableDump]
        bool IsLineSplitRequired
            => IsLineSplitRequiredCache ?? (IsLineSplitRequiredCache = GetIsLineSplitRequired()).Value;

        Context LeftSideContext
            => IsLineSplitRequired
                ? Formatter.LeftSideLineBreakContext(Context)
                : Context.None;

        Context RightSideContext
            => IsLineSplitRequired
                ? Formatter.RightSideLineBreakContext(Context)
                : Context.None;

        bool GetIsLineSplitRequired()
            => Formatter.HasLineBreaksByContext(Context) ||
               Syntax.IsLineBreakRequired(Context.Configuration.EmptyLineLimit, Context.Configuration.MaxLineLength);

        (IEnumerable<ISourcePartEdit> edits, int lineBreaks) GetLeftSide(int minimalLineBreaks)
            => Syntax.Left
                .CreateStruct(LeftSideContext)
                .Get(minimalLineBreaks);

        (IEnumerable<ISourcePartEdit> edits, int lineBreaks) GetRightSide(int minimalLineBreaks)
            => Syntax.Right
                .CreateStruct(RightSideContext)
                .Get(minimalLineBreaks);

        protected override string GetNodeDump() => base.GetNodeDump() + " " + Syntax.Token.Characters.Id;

        static IEnumerable<TValue> T<TValue>(params TValue[] value) => value;
    }
}