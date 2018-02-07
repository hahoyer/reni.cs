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
            public virtual bool? IndentMainAndRightSite => null;
            public virtual bool? IndentLeftSite => null;
            public virtual bool? IndentRightSite => null;
            public virtual bool UseLineBreakBeforeMainWhenForced => false;
            public virtual bool UseLineBreakAfterMainWhenForced => false;
            public virtual bool ForceLineBreakOnLeftSiteWhenForced => false;
            public virtual bool ForceLineBreakOnRightSiteWhenForced => false;
            public virtual bool UseLineBreakBeforeMain => false;
            public virtual bool UseLineBreakAfterMain => false;
            public virtual bool ForceLineBreakOnLeftSite => false;
            public virtual bool ForceLineBreakOnRightSite => false;
        }

        sealed class EmptyFormatter : BaseFormatter {}

        sealed class DefaultFormatter : BaseFormatter {}

        sealed class RightParenthesisFormatter : BaseFormatter
        {
            public override bool UseLineBreakBeforeMainWhenForced => true;
            public override bool UseLineBreakAfterMainWhenForced => true;
            public override bool UseLineBreakBeforeMain => true;
            public override bool ForceLineBreakOnLeftSite => true;
            public override bool ForceLineBreakOnLeftSiteWhenForced => true;
        }

        sealed class LeftParenthesisFormatter : BaseFormatter
        {
            public override bool? IndentRightSite => false;
            public override bool UseLineBreakBeforeMainWhenForced => true;
            public override bool UseLineBreakAfterMainWhenForced => true;
            public override bool UseLineBreakAfterMain => true;
            public override bool ForceLineBreakOnRightSiteWhenForced => true;
            public override bool ForceLineBreakOnRightSite => true;
        }

        sealed class ColonFormatter : BaseFormatter
        {
            public override bool? IndentLeftSite => true;
            public override bool UseLineBreakAfterMainWhenForced => true;
            public override bool UseLineBreakAfterMain => true;
        }

        sealed class ChainFormatter : BaseFormatter
        {
            public override bool? IndentMainAndRightSite => false;
            public override bool UseLineBreakBeforeMainWhenForced => true;
            public override bool ForceLineBreakOnLeftSiteWhenForced => true;
            public override bool UseLineBreakBeforeMain => true;
            public override bool ForceLineBreakOnLeftSite => true;
        }

        abstract class AnyListFormatter : BaseFormatter
        {
            public override bool UseLineBreakAfterMainWhenForced => true;
            public override bool UseLineBreakAfterMain => true;
        }

        sealed class ListFormatter : AnyListFormatter
        {
            public override bool ForceLineBreakOnRightSite => true;
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
                case List _:
                    if(syntax.Right?.TokenClass == syntax.TokenClass)
                        return List;
                    return LastList;
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

        IEnumerable<ISourcePartEdit> IStructure.GetSourcePartEdits(bool exlucdePrefix, bool includeSuffix)
        {
            Tracer.ConditionalBreak
            (
                IsLineBreakRequired &&
                !(Syntax.TokenClass is EndOfText) &&
                !(Syntax.TokenClass is BeginOfText) &&
                !(Syntax.TokenClass is RightParenthesis) &&
                !(Syntax.TokenClass is LeftParenthesis) &&
                !(Syntax.TokenClass is Colon) &&
                !(Syntax.TokenClass is Definable) &&
                !(Syntax.TokenClass is List),
                () => Syntax.TokenClass.Id
            );
            var result = new List<ISourcePartEdit>();
            if(Syntax.Left != null)
                result.AddRange(GetLeftSiteEdits(exlucdePrefix));
            result.AddRange(GetMainAndRightSiteEdits(Syntax.Left == null && exlucdePrefix, includeSuffix));
            return result;
        }

        [EnableDump]
        bool IsLineBreakRequired
            => IsLineBreakRequiredCache ?? (IsLineBreakRequiredCache = GetIsLineBreakRequired()).Value;

        [EnableDump]
        string FlatResult => Syntax.FlatFormat(Parent.Configuration);

        [EnableDumpExcept(exception: false)]
        bool UseLineBreakBeforeMain
            => IsLineBreakForced
                ? Formatter.UseLineBreakBeforeMainWhenForced
                : IsLineBreakRequired && Formatter.UseLineBreakBeforeMain;

        [EnableDumpExcept(exception: false)]
        bool UseLineBreakAfterMain
            => IsLineBreakForced
                ? Formatter.UseLineBreakAfterMainWhenForced
                : IsLineBreakRequired && Formatter.UseLineBreakAfterMain;

        [EnableDumpExcept(exception: false)]
        bool ForceLineBreakOnLeftSite
            => (IsLineBreakForced || IsLineBreakRequired) && Formatter.ForceLineBreakOnLeftSite;

        [EnableDumpExcept(exception: false)]
        bool ForceLineBreakOnRightSite
            => (IsLineBreakForced || IsLineBreakRequired) && Formatter.ForceLineBreakOnRightSite;

        IEnumerable<ISourcePartEdit> BeforeMain
        {
            get
            {
                if(UseLineBreakBeforeMain)
                    yield return SourcePartEditExtension.LineBreak;
            }
        }

        IEnumerable<ISourcePartEdit> AfterMain
        {
            get
            {
                if(UseLineBreakAfterMain)
                    yield return SourcePartEditExtension.LineBreak;
            }
        }

        bool GetIsLineBreakRequired() => Syntax.IsLineBreakRequired(Parent.Configuration);

        IEnumerable<ISourcePartEdit> GetMainAndRightSiteEdits(bool exlucdePrefix, bool includeSuffix)
        {
            var result = new List<ISourcePartEdit>();
            var main = FormatterTokenGroup.Create(Syntax);

            if(!exlucdePrefix)
                result.AddRange(main.Prefix);

            result.AddRange(BeforeMain);
            result.AddRange(main.Main);
            result.AddRange(AfterMain);

            if(Syntax.Right != null || includeSuffix)
                result.AddRange(main.Suffix);

            if(Syntax.Right != null)
                result.AddRange(GetRightSiteEdits(includeSuffix));

            return result.Indent(Formatter.IndentMainAndRightSite);
        }

        IEnumerable<ISourcePartEdit> GetLeftSiteEdits(bool exlucdePrefix)
            => Syntax.Left
                .CreateStruct(Parent, ForceLineBreakOnLeftSite)
                .GetSourcePartEdits(exlucdePrefix, includeSuffix: false)
                .Indent(Formatter.IndentLeftSite);


        IEnumerable<ISourcePartEdit> GetRightSiteEdits(bool includeSuffix)
            => Syntax.Right
                .CreateStruct(Parent, ForceLineBreakOnRightSite)
                .GetSourcePartEdits(exlucdePrefix: true, includeSuffix: includeSuffix)
                .Indent(Formatter.IndentRightSite);


        protected override string GetNodeDump() => base.GetNodeDump() + " " + Syntax.Token.Characters.Id;
    }
}