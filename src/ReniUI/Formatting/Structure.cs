using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class Structure : DumpableObject, IStructure
    {
        [EnableDump]
        readonly Context Context;

        [EnableDump]
        readonly Formatter Formatter;

        readonly Syntax Syntax;

        [EnableDump]
        [EnableDumpExcept(exception: null)]
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
            var trace = True; //Formatter is Formatter.ListItemFormatter;
            StartMethodDump(trace, minimalLineBreaks);
            try
            {
                Tracer.ConditionalBreak(trace);
                var result = new List<ISourcePartEdit>();
                minimalLineBreaks = T(minimalLineBreaks, LineBreaksLeftOfAll).Max();
                if(Syntax.Left != null)
                {
                    minimalLineBreaks = T(minimalLineBreaks, LineBreaksLeftOfLeft).Max();
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
                    lineBreaksOnRightSide = T(rightSide.lineBreaks, LineBreaksRightOfRight).Max();
                }

                lineBreaksOnRightSide = T(lineBreaksOnRightSide, LineBreaksRightOfAll).Max();
                var b = AsString(result.ToArray());
                return ReturnMethodDump((result.ToArray(), lineBreaksOnRightSide), false);
            }
            finally
            {
                EndMethodDump();
            }
        }

        static bool True => true;
        static bool False => false;

        int LineBreaksRightOfAll => HasLineBreaksRightOfAll ? 1 : 0;
        int LineBreaksRightOfRight => HasLineBreaksRightOfRight ? 1 : 0;
        int LineBreaksOnRightSide => HasLineBreaksOnRightSide ? HasMultipleLineBreaksOnRightSide ? 2 : 1 : 0;
        int LineBreaksLeftOfLeft => HasLineBreaksLeftOfLeft ? 1 : 0;
        int LineBreaksLeftOfAll => HasLineBreaksLeftOfAll ? 1 : 0;

        bool RequiresExtraLineBreak => ThisListItemHasLineBreaks || NextListItemHasLineBreaks;

        bool NextListItemHasLineBreaks => Context.Configuration.IsLineBreakRequired(Syntax.Right?.Left);

        bool ThisListItemHasLineBreaks => Context.Configuration.IsLineBreakRequired(Syntax.Left);

        IEnumerable<ISourcePartEdit> GetTokenEdits(int minimalLineBreaks)
        {
            yield return new FormatterTokenView
            (
                Syntax.LeftSideSeparator(),
                minimalLineBreaks,
                LineBreaksOnLeftSide,
                Syntax.Main,
                Context.Configuration,
                Syntax.LeftWhiteSpaces
            );
        }

        public int LineBreaksOnLeftSide => HasLineBreaksOnLeftSide ? 1 : 0;

        string AsString(ISourcePartEdit[] target)
            => target
                .GetEditPieces(Context.Configuration)
                .Combine(Syntax.SourcePart.Source.All);

        [EnableDump]
        string FlatResult => Syntax.FlatFormat(Context.Configuration.EmptyLineLimit);

        bool HasLineBreaksLeftOfLeft
            => IsLineSplitRequired && Formatter.HasLineBreaksLeftOfLeft;

        bool HasLineBreaksLeftOfAll
            => IsLineSplitRequired && Formatter.HasLineBreaksLeftOfAll;

        bool HasLineBreaksRightOfRight
            => IsLineSplitRequired && Formatter.HasLineBreaksRightOfRight;

        bool HasLineBreaksRightOfAll
            => IsLineSplitRequired && Formatter.HasLineBreaksRightOfAll;

        bool HasLineBreaksOnLeftSide
            => IsLineSplitRequired && Formatter.HasLineBreaksBeforeToken(Context);

        bool HasLineBreaksOnRightSide
            => IsLineSplitRequired && Formatter.HasLineBreaksAfterToken(Context);

        bool HasMultipleLineBreaksOnRightSide => Formatter.HasMultipleLineBreaksOnRightSide(BothSidesContext);

        [EnableDump]
        bool IsLineSplitRequired
            => IsLineSplitRequiredCache ?? (IsLineSplitRequiredCache = GetIsLineSplitRequired()).Value;

        Context LeftSideContext
            => IsLineSplitRequired
                ? Formatter.LeftSideLineBreakContext(Context)
                : Context.None;

        Context BothSidesContext
            => Formatter.BothSideContext(Context, Syntax);

        Context RightSideContext
            => IsLineSplitRequired
                ? Formatter.RightSideLineBreakContext(Context)
                : Context.None;

        bool GetIsLineSplitRequired()
            => Formatter.HasLineBreaksByContext(Context) || Context.Configuration.IsLineBreakRequired(Syntax);

        (IEnumerable<ISourcePartEdit> edits, int lineBreaks) GetLeftSide(int minimalLineBreaks)
            => Syntax.Left
                .CreateStruct(LeftSideContext)
                .Get(minimalLineBreaks);

        (IEnumerable<ISourcePartEdit> edits, int lineBreaks) GetRightSide(int minimalLineBreaks)
            => Syntax.Right
                .CreateStruct(RightSideContext)
                .Get(minimalLineBreaks);

        protected override string GetNodeDump() => base.GetNodeDump() + " " + Syntax.Main.Id;

        static IEnumerable<TValue> T<TValue>(params TValue[] value) => value;
    }
}