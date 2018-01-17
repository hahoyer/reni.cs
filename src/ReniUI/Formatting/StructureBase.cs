using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    abstract class StructureBase : DumpableObject, IStructure
    {
        abstract class BaseFormatter : DumpableObject
        {
            public virtual bool? MainAndRightSite=> null;
            public virtual bool? LeftSite => null;
            public virtual bool? RightSite => null;
        }

        class DefaultFormatter : BaseFormatter
        {
            public static readonly BaseFormatter Instance = new DefaultFormatter();
        }

        class LeftParenthesisFormatter : BaseFormatter
        {
            public static readonly BaseFormatter Instance = new LeftParenthesisFormatter();
            public override bool? RightSite => false;
        }

        class ColonFormatter : BaseFormatter
        {
            public static readonly BaseFormatter Instance = new ColonFormatter();
            public override bool? LeftSite => true;
        }

        class ChainFormatter : BaseFormatter
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

        readonly ValueCache<bool> IsLineBreakRequiredCache;
        protected readonly StructFormatter Parent;
        protected readonly Syntax Syntax;

        protected StructureBase(Syntax syntax, StructFormatter parent)
        {
            Formatter = CreateFormatter(syntax);
            Syntax = syntax;
            Parent = parent;
            IsLineBreakRequiredCache = new ValueCache<bool>(() => Syntax.IsLineBreakRequired(Parent.Configuration));
            Tracer.Assert(Syntax != null);
        }

        Syntax IStructure.Syntax => Syntax;

        IEnumerable<ISourcePartEdit>
            IStructure.GetSourcePartEdits(bool exlucdePrefix, bool includeSuffix)
        {
            var lr = IsLineBreakRequired;
            var result = new List<ISourcePartEdit>();

            if(Syntax.Left != null)
                result.AddRange(GetLeftSiteEdits(exlucdePrefix));

            result.AddRange(GetMainAndRightSiteEdits(includeSuffix));
            AssertValid(result);
            return result;
        }

        [EnableDump]
        protected bool IsLineBreakRequired => IsLineBreakRequiredCache.Value;

        [EnableDump]
        protected string FlatResult => Syntax.FlatFormat(Parent.Configuration);

        IEnumerable<ISourcePartEdit> GetMainAndRightSiteEdits(bool includeSuffix)
        {
            var main = FormatterTokenGroup.Create(Syntax);

            IEnumerable<ISourcePartEdit> result = main.Main;

            if(Syntax.Right != null || includeSuffix)
                result = result.Concat(main.Suffix);

            if(Syntax.Right != null)
                result = result.Concat(GetRightSiteEdits(includeSuffix));

            return result.Indent(Formatter.MainAndRightSite);
        }

        IEnumerable<ISourcePartEdit> GetLeftSiteEdits(bool exlucdePrefix)
            => Syntax.Left
                .CreateStruct(Parent)
                .GetSourcePartEdits(exlucdePrefix, includeSuffix: false)
                .Indent(Formatter.LeftSite);

        IEnumerable<ISourcePartEdit> GetRightSiteEdits(bool includeSuffix)
            => Syntax.Right
                .CreateStruct(Parent)
                .GetSourcePartEdits(exlucdePrefix: true, includeSuffix: includeSuffix)
                .Indent(Formatter.RightSite);

        protected virtual IEnumerable<IEnumerable<ISourcePartEdit>>
            GetSourcePartEdits(SourcePart targetPart, bool exlucdePrefix) => null;

        protected override string GetNodeDump() => base.GetNodeDump() + " " + Syntax.Token.Characters.Id;
    }
}