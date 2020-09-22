using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
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
        [EnableDumpExcept(null)]
        bool? IsLineSplitRequiredCache;


        internal Structure(Syntax syntax, Context context)
        {
            Syntax = syntax;
            Context = context;
            Formatter = Formatter.CreateFormatter(Syntax);
            Tracer.Assert(Syntax != null);
        }

        int IStructure.LineBreaks => LineBreaks;

        IEnumerable<ISourcePartEdit> IStructure.Edits
        {
            get
            {
                var trace = False;
                StartMethodDump(trace);
                try
                {
                    var result = Edits.SelectMany(i => i).ToArray();
                    Dump(nameof(result), result);
                    Tracer.ConditionalBreak(trace);
                    return ReturnMethodDump(result, trace);
                }
                finally
                {
                    EndMethodDump();
                }
            }
        }

        IEnumerable<IEnumerable<ISourcePartEdit>> Edits
        {
            get
            {
                if(Syntax.Left != null)
                {
                    yield return LeftEdits.Indent(Formatter.IndentLeftSide);
                    yield return LeftWhiteSpacesEdits.Indent(Formatter.IndentToken);
                }

                if(Syntax.Right != null)
                {
                    yield return RightWhiteSpacesEdits.Indent(Formatter.IndentToken);
                    yield return RightEdits.Indent(Formatter.IndentRightSide);
                }
            }
        }

        [DisableDump]
        bool Trace =>
            !(Syntax.TokenClass is EndOfText) &&
            !(Syntax.TokenClass is BeginOfText) &&
            !(Syntax.TokenClass is UserSymbol);


        [DisableDump]
        int LineBreaks
        {
            get
            {
                if(Trace)
                {
                    nameof(Syntax.TokenClass).DumpValue(Syntax.TokenClass).WriteLine();
                    Tracer.ConditionalBreak(True);
                }

                return 0;
            }
        }

        static bool True => true;
        static bool False => false;

        int LineBreaksRightOfAll => HasLineBreaksRightOfAll ? 1 : 0;
        int LineBreaksRightOfRight => HasLineBreaksRightOfRight ? 1 : 0;
        int LineBreaksOnRightSide => HasLineBreaksOnRightSide ? HasMultipleLineBreaksOnRightSide ? 2 : 1 : 0;
        int LineBreaksLeftOfLeft => HasLineBreaksLeftOfLeft ? 1 : 0;

        bool RequiresExtraLineBreak => ThisListItemHasLineBreaks || NextListItemHasLineBreaks;

        bool NextListItemHasLineBreaks => Context.Configuration.IsLineBreakRequired(Syntax.Right?.Left);

        bool ThisListItemHasLineBreaks => Context.Configuration.IsLineBreakRequired(Syntax.Left);

        public int LineBreaksOnLeftSide => HasLineBreaksOnLeftSide ? 1 : 0;

        string AsString(ISourcePartEdit[] target)
            => target
                .GetEditPieces(Context.Configuration)
                .Combine(Syntax.Target.Option.SourcePart.Source.All);

        [EnableDump]
        string FlatResult => Syntax.FlatFormat(Context.Configuration.EmptyLineLimit != 0);

        bool HasLineBreaksLeftOfLeft
            => IsLineSplitRequired && Formatter.HasLineBreaksLeftOfLeft;

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

        int LeftSideLineBreaks =>
            Syntax.Left
                .CreateStruct(LeftSideContext)
                .LineBreaks;

        IEnumerable<ISourcePartEdit> LeftEdits =>
            Syntax.Left
                .CreateStruct(LeftSideContext)
                .Edits;

        int RightSideLineBreaks =>
            Syntax.Right
                .CreateStruct(RightSideContext)
                .LineBreaks;

        IEnumerable<ISourcePartEdit> RightEdits =>
            Syntax.Right
                .CreateStruct(RightSideContext)
                .Edits;

        IEnumerable<ISourcePartEdit> LeftWhiteSpacesEdits
        {
            get
            {
                yield return GetWhiteSpaceView
                (
                    Syntax.LeftWhiteSpaces,
                    Syntax.MainToken.Start,
                    Syntax.LeftSideSeparator(),
                    LineBreaksOnLeftSide);
            }
        }

        IEnumerable<ISourcePartEdit> RightWhiteSpacesEdits
        {
            get
            {
                yield return GetWhiteSpaceView
                (
                    Syntax.RightWhiteSpaces,
                    Syntax.MainToken.End,
                    Syntax.RightSideSeparator(),
                    LineBreaksOnRightSide);
            }
        }

        ISourcePartEdit GetWhiteSpaceView
        (
            IEnumerable<IItem> target,
            SourcePosn anchor,
            bool isSeparatorRequired,
            int minimalLineBreakCount)
        {
            if(target.Any())
                return new WhiteSpaceView
                (
                    target,
                    Context.Configuration,
                    isSeparatorRequired,
                    minimalLineBreakCount);
            
            return new EmptyWhiteSpaceView
            (
                anchor,
                isSeparatorRequired,
                minimalLineBreakCount);
        }


        protected override string GetNodeDump() => base.GetNodeDump() + " " + Syntax.MainToken.Id;

        static IEnumerable<TValue> T<TValue>(params TValue[] value) => value;
    }
}