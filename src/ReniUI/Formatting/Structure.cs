using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class Structure : DumpableObject, IStructure
    {
        readonly Context Context;

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

        IEnumerable<ISourcePartEdit> IStructure.GetSourcePartEdits()
        {
            var result = new List<ISourcePartEdit>();
            if(Syntax.Left!= null)
                result.AddRange(GetLeftSiteEdits().Indent(Formatter.IndentLeftSide));
            result.AddRange(GetTokenEdits().Indent(Formatter.IndentToken));
            if(Syntax.Right != null)
                result.AddRange(GetRightSiteEdits().Indent(Formatter.IndentRightSide));

            var b = AsString(result.ToArray());
            return result.ToArray();
        }

        IEnumerable<ISourcePartEdit> GetTokenEdits()
        {
            ISeparatorType leftSeparator = Syntax.LeftSideSeparator();
            yield return new FormatterTokenView(leftSeparator, LineBreaksBeforeToken,Syntax.Token, LineBreaksAfterToken, Context.Configuration);
        }

        string AsString(ISourcePartEdit[] tokenAndRightSideEdits)
            => tokenAndRightSideEdits
                .GetEditPieces(Context.Configuration)
                .Combine(Syntax.SourcePart.Source.All);

        [EnableDump]
        string FlatResult => Syntax.FlatFormat(Context.Configuration.EmptyLineLimit);

        int LineBreaksBeforeToken 
            => IsLineSplitRequired ? Formatter.LineBreaksBeforeToken(Context) : 0;

        int LineBreaksAfterToken 
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

        IEnumerable<ISourcePartEdit> GetLeftSiteEdits()
            => Syntax.Left
                .CreateStruct(LeftSideContext)
                .GetSourcePartEdits()
                .Indent(Formatter.IndentLeftSide);

        IEnumerable<ISourcePartEdit> GetRightSiteEdits()
            => Syntax.Right
                .CreateStruct(RightSideContext)
                .GetSourcePartEdits()
                .Indent(Formatter.IndentRightSide);

        protected override string GetNodeDump() => base.GetNodeDump() + " " + Syntax.Token.Characters.Id;

        static IEnumerable<TValue> T<TValue>(params TValue[] value) => value;
    }
}