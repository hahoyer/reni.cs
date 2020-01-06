using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    abstract class StructureBase : DumpableObject, IStructure
    {
        abstract class BaseFormatter : DumpableObject
        {
            public virtual bool? MainAndRightSite => null;
            public virtual bool? LeftSite => null;
            public virtual bool? RightSite => null;
        }

        sealed class DefaultFormatter : BaseFormatter
        {
            public static readonly BaseFormatter Instance = new DefaultFormatter();
        }

        sealed class LeftParenthesisFormatter : BaseFormatter
        {
            public static readonly BaseFormatter Instance = new LeftParenthesisFormatter();
            public override bool? RightSite => false;
        }

        sealed class ColonFormatter : BaseFormatter
        {
            public static readonly BaseFormatter Instance = new ColonFormatter();
            public override bool? LeftSite => true;
        }

        sealed class ChainFormatter : BaseFormatter
        {
            public static readonly BaseFormatter Instance = new ChainFormatter();
            public override bool? MainAndRightSite => false;
        }

        static void AssertValid(IEnumerable<ISourcePartEdit> result)
        {
            var currentPosition = 0;
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
                case LeftParenthesis _: return LeftParenthesisFormatter.Instance;
                case Colon _: return ColonFormatter.Instance;
                case Definable _:
                    if(syntax.Left != null)
                        return ChainFormatter.Instance;
                    break;
            }

            return DefaultFormatter.Instance;
        }

        readonly BaseFormatter Formatter;
        readonly StructFormatter Parent;
        readonly Syntax Syntax;

        bool? IsLineBreakRequiredCache;

        protected StructureBase(Syntax syntax, StructFormatter parent, bool isLineBreakRequired)
        {
            Formatter = CreateFormatter(syntax);
            Syntax = syntax;
            Parent = parent;
            IsLineBreakRequiredCache = isLineBreakRequired ? true : (bool?) null;
            Tracer.Assert(Syntax != null);
        }

        Syntax IStructure.Syntax => Syntax;

        IEnumerable<ISourcePartEdit>
            IStructure.GetSourcePartEdits(bool excludePrefix, bool includeSuffix)
        {
            var result = new List<ISourcePartEdit>();

            if(Syntax.Left != null)
                result.AddRange(GetLeftSiteEdits(excludePrefix));

            result.AddRange(GetMainAndRightSiteEdits(includeSuffix));
            AssertValid(result);
            return result;
        }

        [EnableDump]
        protected bool IsLineBreakRequired
            => IsLineBreakRequiredCache ?? (IsLineBreakRequiredCache = GetIsLineBreakRequired()).Value;

        [EnableDump]
        protected string FlatResult => Syntax.FlatFormat(Parent.Configuration.EmptyLineLimit);

        bool GetIsLineBreakRequired() => Syntax.IsLineBreakRequired(Parent.Configuration.EmptyLineLimit, Parent.Configuration.MaxLineLength);

        IEnumerable<ISourcePartEdit> GetMainAndRightSiteEdits(bool includeSuffix)
        {
            var main = FormatterTokenGroup.Create(Syntax);

            IEnumerable<ISourcePartEdit> result = main.TokenEdits;

            if(Syntax.Right != null || includeSuffix)
                result = result.Concat(main.SuffixEdits);

            if(Syntax.Right != null)
                result = result.Concat(GetRightSiteEdits(includeSuffix));

            return result.Indent(Formatter.MainAndRightSite);
        }

        IEnumerable<ISourcePartEdit> GetLeftSiteEdits(bool excludePrefix)
            => Syntax.Left
                .CreateStruct(Parent, false)
                .GetSourcePartEdits(excludePrefix, false)
                .Indent(Formatter.LeftSite);

        IEnumerable<ISourcePartEdit> GetRightSiteEdits(bool includeSuffix)
            => Syntax.Right
                .CreateStruct(Parent, false)
                .GetSourcePartEdits(true, includeSuffix)
                .Indent(Formatter.RightSite);

        protected override string GetNodeDump() => base.GetNodeDump() + " " + Syntax.Token.Characters.Id;
    }
}