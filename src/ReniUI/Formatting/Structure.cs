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

        readonly Formatter Formatter;
        readonly FormatterMode Mode;
        readonly StructFormatter Parent;
        readonly Syntax Syntax;

        bool? IsLineBreakRequiredCache;

        internal Structure(Syntax syntax, StructFormatter parent, FormatterMode mode)
        {
            Formatter = Formatter.CreateFormatter(syntax);
            Syntax = syntax;
            Parent = parent;
            Mode = mode;
            Tracer.Assert(Syntax != null);
        }

        Syntax IStructure.Syntax => Syntax;

        IEnumerable<ISourcePartEdit> IStructure.GetSourcePartEdits(bool excludePrefix, bool includeSuffix)
        {
            var result = new List<ISourcePartEdit>();
            if(Syntax.Left != null)
                result.AddRange(GetLeftSiteEdits(excludePrefix));
            result.AddRange(GetTokenAndRightSiteEdits(Syntax.Left == null && excludePrefix, includeSuffix));
            return result;
        }

        [EnableDump]
        bool IsLineBreakRequired
            => IsLineBreakRequiredCache ?? (IsLineBreakRequiredCache = GetIsLineBreakRequired()).Value;

        [EnableDump]
        string FlatResult => Syntax.FlatFormat(Parent.Configuration.EmptyLineLimit);

        IEnumerable<ISourcePartEdit> EditsBetweenPrefixAndToken
        {
            get
            {
                if((Mode.HasLineBreakForced || IsLineBreakRequired) && Formatter.UseLineBreakBeforeToken(Mode))
                    yield return SourcePartEditExtension.LineBreak;
            }
        }

        IEnumerable<ISourcePartEdit> EditsBetweenTokenAndSuffix
        {
            get
            {
                if((Mode.HasLineBreakForced || IsLineBreakRequired) && Formatter.UseLineBreakAfterToken(Mode))
                    yield return SourcePartEditExtension.LineBreak;
            }
        }

        FormatterMode LeftSideMode
            => Mode.HasLineBreakForced || IsLineBreakRequired
                ? Formatter.LeftSideWithLineBreaksMode(Mode)
                : FormatterMode.None;

        FormatterMode RightSideMode
            => Mode.HasLineBreakForced || IsLineBreakRequired
                ? Formatter.RightSideWithLineBreaksMode(Mode)
                : FormatterMode.None;

        bool GetIsLineBreakRequired()
            => Syntax.IsLineBreakRequired(Parent.Configuration.EmptyLineLimit, Parent.Configuration.MaxLineLength);

        IEnumerable<ISourcePartEdit> GetTokenAndRightSiteEdits(bool excludePrefix, bool includeSuffix)
        {
            var result = new List<ISourcePartEdit>();
            var token = FormatterTokenGroup.Create(Syntax);

            if(!excludePrefix)
                result.AddRange(token.PrefixEdits);

            result.AddRange(EditsBetweenPrefixAndToken);
            result.AddRange(token.TokenEdits);
            result.AddRange(EditsBetweenTokenAndSuffix);

            if(Syntax.Right != null || includeSuffix)
                result.AddRange(token.SuffixEdits);

            if(Syntax.Right != null)
                result.AddRange(GetRightSiteEdits(includeSuffix));

            return result.Indent(Formatter.IndentTokenAndRightSite);
        }

        IEnumerable<ISourcePartEdit> GetLeftSiteEdits(bool excludePrefix)
            => Syntax.Left
                .CreateStruct(Parent, LeftSideMode)
                .GetSourcePartEdits(excludePrefix, includeSuffix: false)
                .Indent(Formatter.IndentLeftSite);

        IEnumerable<ISourcePartEdit> GetRightSiteEdits(bool includeSuffix)
            => Syntax.Right
                .CreateStruct(Parent, RightSideMode)
                .GetSourcePartEdits(excludePrefix: true, includeSuffix: includeSuffix)
                .Indent(Formatter.IndentRightSite);

        protected override string GetNodeDump() => base.GetNodeDump() + " " + Syntax.Token.Characters.Id;
    }
}