using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class Structure : DumpableObject, IStructure
    {
        abstract class BaseFormatter : DumpableObject
        {
            public virtual IndentDirection IndentTokenAndRightSite => IndentDirection.NoIndent;
            public virtual IndentDirection IndentLeftSite => IndentDirection.NoIndent;
            public virtual IndentDirection IndentRightSite => IndentDirection.NoIndent;
            public virtual bool UseLineBreakBeforeToken(bool isLineBreakForced) => false;
            public virtual bool UseLineBreakAfterToken(bool isLineBreakForced) => false;
            public virtual bool ForceLineBreakOnLeftSite(bool isLineBreakForced) => false;
            public virtual bool ForceLineBreakOnRightSite(bool isLineBreakForced) => false;
        }

        sealed class EmptyFormatter : BaseFormatter {}

        sealed class DefaultFormatter : BaseFormatter
        {
            public override bool UseLineBreakBeforeToken(bool isLineBreakForced)
            {
                if(isLineBreakForced)
                    NotImplementedFunction(isLineBreakForced);
                return base.UseLineBreakBeforeToken(isLineBreakForced);
            }

            public override bool UseLineBreakAfterToken(bool isLineBreakForced)
            {
                if(isLineBreakForced)
                    NotImplementedFunction(isLineBreakForced);
                return base.UseLineBreakAfterToken(isLineBreakForced);
            }

            public override bool ForceLineBreakOnLeftSite(bool isLineBreakForced)
            {
                NotImplementedFunction(isLineBreakForced);
                return base.ForceLineBreakOnLeftSite(isLineBreakForced);
            }

            public override bool ForceLineBreakOnRightSite(bool isLineBreakForced)
            {
                NotImplementedFunction(isLineBreakForced);
                return base.ForceLineBreakOnRightSite(isLineBreakForced);
            }
        }

        sealed class RightParenthesisFormatter : BaseFormatter
        {
            public override bool UseLineBreakBeforeToken(bool isLineBreakForced) => true;
            public override bool UseLineBreakAfterToken(bool isLineBreakForced) => isLineBreakForced;
            public override bool ForceLineBreakOnLeftSite(bool isLineBreakForced) => isLineBreakForced;
        }

        sealed class LeftParenthesisFormatter : BaseFormatter
        {
            public override IndentDirection IndentRightSite => IndentDirection.ToRight;
            public override bool UseLineBreakBeforeToken(bool isLineBreakForced) => isLineBreakForced;
            public override bool UseLineBreakAfterToken(bool isLineBreakForced) => true;
            public override bool ForceLineBreakOnRightSite(bool isLineBreakForced) => true;

            public override bool ForceLineBreakOnLeftSite(bool isLineBreakForced)
            {
                NotImplementedFunction(isLineBreakForced);
                return base.ForceLineBreakOnLeftSite(isLineBreakForced);
            }
        }

        sealed class ColonFormatter : BaseFormatter
        {
            public override IndentDirection IndentLeftSite => IndentDirection.ToLeft;

            public override bool UseLineBreakBeforeToken(bool isLineBreakForced)
            {
                if(isLineBreakForced)
                    NotImplementedFunction(isLineBreakForced);
                return base.UseLineBreakBeforeToken(isLineBreakForced);
            }

            public override bool UseLineBreakAfterToken(bool isLineBreakForced)
            {
                if(isLineBreakForced)
                    NotImplementedFunction(isLineBreakForced);
                return base.UseLineBreakAfterToken(isLineBreakForced);
            }

            public override bool ForceLineBreakOnLeftSite(bool isLineBreakForced) => false;

            public override bool ForceLineBreakOnRightSite(bool isLineBreakForced)
            {
                if(isLineBreakForced)
                    NotImplementedFunction(isLineBreakForced);
                return base.ForceLineBreakOnRightSite(isLineBreakForced);
            }
        }

        sealed class ChainFormatter : BaseFormatter
        {
            public override IndentDirection IndentTokenAndRightSite => IndentDirection.ToRight;
            public override bool ForceLineBreakOnLeftSite(bool isLineBreakForced) => isLineBreakForced;
            public override bool ForceLineBreakOnRightSite(bool isLineBreakForced) => isLineBreakForced;
        }

        abstract class AnyListFormatter : BaseFormatter
        {
            public override bool UseLineBreakAfterToken(bool isLineBreakForced) => isLineBreakForced;
        }

        sealed class ListFormatter : AnyListFormatter
        {
            public override bool ForceLineBreakOnRightSite(bool isLineBreakForced) => true;
        }

        sealed class LastListFormatter : AnyListFormatter {}

        static readonly BaseFormatter Empty = new EmptyFormatter();
        static readonly BaseFormatter Default = new DefaultFormatter();
        static readonly BaseFormatter RightParenthesis = new RightParenthesisFormatter();
        static readonly BaseFormatter LeftParenthesis = new LeftParenthesisFormatter();
        static readonly BaseFormatter Colon = new ColonFormatter();
        static readonly BaseFormatter Chain = new ChainFormatter();
        static readonly BaseFormatter List = new ListFormatter();
        static readonly BaseFormatter LastList = new LastListFormatter();

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

        static BaseFormatter CreateFormatter(Syntax syntax)
        {
            switch(syntax.TokenClass)
            {
                case BeginOfText _:
                case EndOfText _: return Empty;
                case LeftParenthesis _: return LeftParenthesis;
                case RightParenthesis _: return RightParenthesis;
                case Colon _: return Colon;
                case Definable _:
                    if(syntax.Left != null)
                        return Chain;
                    break;
                case List _: return syntax.Right?.TokenClass == syntax.TokenClass ? List : LastList;
            }

            return Default;
        }


        readonly BaseFormatter Formatter;
        readonly bool IsLineBreakForced;
        readonly StructFormatter Parent;
        readonly Syntax Syntax;

        bool? IsLineBreakRequiredCache;

        internal Structure(Syntax syntax, StructFormatter parent, bool isLineBreakForced)
        {
            Formatter = CreateFormatter(syntax);
            Syntax = syntax;
            Parent = parent;
            IsLineBreakForced = isLineBreakForced;
            Tracer.Assert(Syntax != null);
        }

        Syntax IStructure.Syntax => Syntax;

        IEnumerable<ISourcePartEdit> IStructure.GetSourcePartEdits(bool excludePrefix, bool includeSuffix)
        {
            var result = new List<ISourcePartEdit>();
            if(Syntax.Left != null)
                result.AddRange(GetLeftSiteEdits(excludePrefix));
            result.AddRange(GetMainAndRightSiteEdits(Syntax.Left == null && excludePrefix, includeSuffix));
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
                if((IsLineBreakForced || IsLineBreakRequired) && Formatter.UseLineBreakBeforeToken(IsLineBreakForced))
                    yield return SourcePartEditExtension.LineBreak;
            }
        }

        IEnumerable<ISourcePartEdit> EditsBetweenTokenAndSuffix
        {
            get
            {
                if((IsLineBreakForced || IsLineBreakRequired) && Formatter.UseLineBreakAfterToken(IsLineBreakForced))
                    yield return SourcePartEditExtension.LineBreak;
            }
        }

        bool ForceLineBreakOnLeftSize
            => (IsLineBreakForced || IsLineBreakRequired) && Formatter.ForceLineBreakOnLeftSite(IsLineBreakForced);

        bool ForceLineBreakOnRightSize
            => (IsLineBreakForced || IsLineBreakRequired) && Formatter.ForceLineBreakOnRightSite(IsLineBreakForced);

        bool GetIsLineBreakRequired() => Syntax.IsLineBreakRequired
            (Parent.Configuration.EmptyLineLimit, Parent.Configuration.MaxLineLength);

        IEnumerable<ISourcePartEdit> GetMainAndRightSiteEdits(bool excludePrefix, bool includeSuffix)
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
                .CreateStruct(Parent, ForceLineBreakOnLeftSize)
                .GetSourcePartEdits(excludePrefix, includeSuffix: false)
                .Indent(Formatter.IndentLeftSite);

        IEnumerable<ISourcePartEdit> GetRightSiteEdits(bool includeSuffix)
            => Syntax.Right
                .CreateStruct(Parent, ForceLineBreakOnRightSize)
                .GetSourcePartEdits(excludePrefix: true, includeSuffix: includeSuffix)
                .Indent(Formatter.IndentRightSite);

        protected override string GetNodeDump() => base.GetNodeDump() + " " + Syntax.Token.Characters.Id;
    }
}