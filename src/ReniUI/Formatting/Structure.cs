using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class Structure : DumpableObject, IStructure
    {
        static void AssertValid(IEnumerable<ISourcePartEdit> result)
        {
            var currentPosition = 0;
            // ReSharper disable once NotAccessedVariable, is for debugging purpose
            var currentIndex = 0;

            foreach(var edit in result)
            {
                if(edit is SourcePartEdit s)
                {
                    var part = ((ISourcePartProxy) s.Source).All;
                    Tracer.Assert(part.Position >= currentPosition);
                    currentPosition = part.EndPosition;
                }

                currentIndex++;
            }
        }

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

        IEnumerable<ISourcePartEdit> IStructure.GetSourcePartEdits(bool excludePrefix, bool includeSuffix)
        {
            var token = FormatterTokenGroup.Create(Syntax);

            var tokenAndRightSideEdits = new List<ISourcePartEdit>();

            if(!(Syntax.Left == null && excludePrefix))
                tokenAndRightSideEdits.AddRange(token.PrefixEdits);

            tokenAndRightSideEdits.AddRange(EditsBeforeToken);
            tokenAndRightSideEdits.AddRange(token.TokenEdits);
            tokenAndRightSideEdits.AddRange(EditsAfterToken);

            if(Syntax.Right != null || includeSuffix)
                tokenAndRightSideEdits.AddRange(token.SuffixEdits);

            if(Syntax.Right != null)
                tokenAndRightSideEdits.AddRange(GetRightSiteEdits(includeSuffix));

            var result = new List<ISourcePartEdit>();
            if(Syntax.Left != null)
                result.AddRange(GetLeftSiteEdits(excludePrefix));

            result.AddRange(tokenAndRightSideEdits.Indent(Formatter.IndentTokenAndRightSide));

            var trace = result.GetEditPieces(Context.Configuration);
            return result;
        }

        [EnableDump]
        string FlatResult => Syntax.FlatFormat(Context.Configuration.EmptyLineLimit);

        IEnumerable<ISourcePartEdit> EditsBeforeToken
        {
            get
            {
                if(IsLineSplitRequired && Formatter.UseLineBreakBeforeToken(Context))
                    yield return SourcePartEditExtension.LineBreak;
            }
        }

        IEnumerable<ISourcePartEdit> EditsAfterToken
        {
            get
            {
                if(IsLineSplitRequired && Formatter.UseLineBreakAfterToken(Context))
                    yield return SourcePartEditExtension.LineBreak;
            }
        }

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

        IEnumerable<ISourcePartEdit> GetLeftSiteEdits(bool excludePrefix)
            => Syntax.Left
                .CreateStruct(LeftSideContext)
                .GetSourcePartEdits(excludePrefix, includeSuffix: false)
                .Indent(Formatter.IndentLeftSide);

        IEnumerable<ISourcePartEdit> GetRightSiteEdits(bool includeSuffix)
            => Syntax.Right
                .CreateStruct(RightSideContext)
                .GetSourcePartEdits(excludePrefix: true, includeSuffix: includeSuffix)
                .Indent(Formatter.IndentRightSide);

        protected override string GetNodeDump() => base.GetNodeDump() + " " + Syntax.Token.Characters.Id;
    }
}