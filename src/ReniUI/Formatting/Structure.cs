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
            public virtual bool? MainAndRightSite => null;
            public virtual bool? LeftSite => null;
            public virtual bool? RightSite => null;
            public virtual bool UseLineBreakBeforeMain(bool isLineBreakForced) => false;
            public virtual bool UseLineBreakAfterMain(bool isLineBreakForced) => false;
            public virtual bool ForceLineBreakOnLeftSize(bool isLineBreakForced) => false;
            public virtual bool ForceLineBreakOnRightSize(bool isLineBreakForced) => false;
        }

        sealed class EmptyFormatter : BaseFormatter {}

        sealed class DefaultFormatter : BaseFormatter
        {
            public override bool UseLineBreakBeforeMain(bool isLineBreakForced)
            {
                NotImplementedFunction(isLineBreakForced);
                return base.UseLineBreakBeforeMain(isLineBreakForced);
            }

            public override bool UseLineBreakAfterMain(bool isLineBreakForced)
            {
                NotImplementedFunction(isLineBreakForced);
                return base.UseLineBreakAfterMain
                    (isLineBreakForced);
            }

            public override bool ForceLineBreakOnLeftSize(bool isLineBreakForced)
            {
                NotImplementedFunction(isLineBreakForced);
                return base.ForceLineBreakOnLeftSize(isLineBreakForced);
            }

            public override bool ForceLineBreakOnRightSize(bool isLineBreakForced)
            {
                NotImplementedFunction(isLineBreakForced);
                return base.ForceLineBreakOnRightSize(isLineBreakForced);
            }
        }

        sealed class RightParenthesisFormatter : BaseFormatter
        {
            public override bool UseLineBreakBeforeMain(bool isLineBreakForced) => true;
            public override bool UseLineBreakAfterMain(bool isLineBreakForced) => isLineBreakForced;
            public override bool ForceLineBreakOnLeftSize(bool isLineBreakForced) => isLineBreakForced;
        }

        sealed class LeftParenthesisFormatter : BaseFormatter
        {
            public override bool? RightSite => false;
            public override bool UseLineBreakBeforeMain(bool isLineBreakForced) => isLineBreakForced;
            public override bool UseLineBreakAfterMain(bool isLineBreakForced) => true;
            public override bool ForceLineBreakOnRightSize(bool isLineBreakForced) => true;
        }

        sealed class ColonFormatter : BaseFormatter
        {
            public override bool? LeftSite => true;

            public override bool UseLineBreakBeforeMain(bool isLineBreakForced)
            {
                NotImplementedFunction(isLineBreakForced);
                return base.UseLineBreakBeforeMain(isLineBreakForced);
            }

            public override bool UseLineBreakAfterMain(bool isLineBreakForced)
            {
                NotImplementedFunction(isLineBreakForced);
                return base.UseLineBreakAfterMain
                    (isLineBreakForced);
            }

            public override bool ForceLineBreakOnLeftSize(bool isLineBreakForced)
            {
                NotImplementedFunction(isLineBreakForced);
                return base.ForceLineBreakOnLeftSize(isLineBreakForced);
            }

            public override bool ForceLineBreakOnRightSize(bool isLineBreakForced)
            {
                NotImplementedFunction(isLineBreakForced);
                return base.ForceLineBreakOnRightSize(isLineBreakForced);
            }
        }

        sealed class ChainFormatter : BaseFormatter
        {
            public override bool? MainAndRightSite => false;

            public override bool UseLineBreakBeforeMain(bool isLineBreakForced)
            {
                NotImplementedFunction(isLineBreakForced);
                return base.UseLineBreakBeforeMain(isLineBreakForced);
            }

            public override bool UseLineBreakAfterMain(bool isLineBreakForced)
            {
                NotImplementedFunction(isLineBreakForced);
                return base.UseLineBreakAfterMain
                    (isLineBreakForced);
            }

            public override bool ForceLineBreakOnLeftSize(bool isLineBreakForced)
            {
                NotImplementedFunction(isLineBreakForced);
                return base.ForceLineBreakOnLeftSize(isLineBreakForced);
            }

            public override bool ForceLineBreakOnRightSize(bool isLineBreakForced)
            {
                NotImplementedFunction(isLineBreakForced);
                return base.ForceLineBreakOnRightSize(isLineBreakForced);
            }
        }

        abstract class AnyListFormatter : BaseFormatter
        {
            public override bool UseLineBreakAfterMain(bool isLineBreakForced) => isLineBreakForced;
        }

        sealed class ListFormatter : AnyListFormatter
        {
            public override bool ForceLineBreakOnRightSize(bool isLineBreakForced) => true;
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

        IEnumerable<ISourcePartEdit> IStructure.GetSourcePartEdits(bool excludePrefix, bool includeSuffix)
        {
            Tracer.ConditionalBreak
            (
                IsLineBreakRequired &&
                !(Syntax.TokenClass is EndOfText) &&
                !(Syntax.TokenClass is BeginOfText) &&
                !(Syntax.TokenClass is RightParenthesis) &&
                !(Syntax.TokenClass is LeftParenthesis) &&
                !(Syntax.TokenClass is List),
                () => Syntax.TokenClass.Id
            );
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
        string FlatResult => Syntax.FlatFormat(Parent.Configuration);

        IEnumerable<ISourcePartEdit> BeforeMain
        {
            get
            {
                if((IsLineBreakForced || IsLineBreakRequired) && Formatter.UseLineBreakBeforeMain(IsLineBreakForced))
                    yield return SourcePartEditExtension.LineBreak;
            }
        }

        IEnumerable<ISourcePartEdit> AfterMain
        {
            get
            {
                if((IsLineBreakForced || IsLineBreakRequired) && Formatter.UseLineBreakAfterMain(IsLineBreakForced))
                    yield return SourcePartEditExtension.LineBreak;
            }
        }

        bool ForceLineBreakOnLeftSize
            => (IsLineBreakForced || IsLineBreakRequired) && Formatter.ForceLineBreakOnLeftSize(IsLineBreakForced);

        bool ForceLineBreakOnRightSize
            => (IsLineBreakForced || IsLineBreakRequired) && Formatter.ForceLineBreakOnRightSize(IsLineBreakForced);


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

            return result.Indent(Formatter.MainAndRightSite);
        }

        IEnumerable<ISourcePartEdit> GetLeftSiteEdits(bool exlucdePrefix)
            => Syntax.Left
                .CreateStruct(Parent, ForceLineBreakOnLeftSize)
                .GetSourcePartEdits(exlucdePrefix, false)
                .Indent(Formatter.LeftSite);


        IEnumerable<ISourcePartEdit> GetRightSiteEdits(bool includeSuffix)
            => Syntax.Right
                .CreateStruct(Parent, ForceLineBreakOnRightSize)
                .GetSourcePartEdits(true, includeSuffix)
                .Indent(Formatter.RightSite);


        protected override string GetNodeDump() => base.GetNodeDump() + " " + Syntax.Token.Characters.Id;
    }
}