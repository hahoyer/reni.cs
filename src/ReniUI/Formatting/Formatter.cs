using hw.DebugFormatter;
using hw.Helper;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    abstract class Formatter : EnumEx
    {
        sealed class EmptyFormatter : Formatter {}

        sealed class DefaultFormatter : Formatter
        {
            public override bool UseLineBreakBeforeToken(FormatterMode mode)
            {
                NotImplementedFunction(mode);
                return base.UseLineBreakBeforeToken(mode);
            }

            public override bool UseLineBreakAfterToken(FormatterMode mode)
            {
                NotImplementedFunction(mode);
                return base.UseLineBreakAfterToken(mode);
            }

            public override FormatterMode LeftSideWithLineBreaksMode(FormatterMode mode)
            {
                NotImplementedFunction(mode);
                return base.LeftSideWithLineBreaksMode(mode);
            }

            public override FormatterMode RightSideWithLineBreaksMode(FormatterMode mode)
            {
                NotImplementedFunction(mode);
                return base.RightSideWithLineBreaksMode(mode);
            }
        }

        sealed class RightParenthesisFormatter : Formatter
        {
            public override bool UseLineBreakBeforeToken(FormatterMode mode) => !mode.IsRoot;
            public override bool UseLineBreakAfterToken(FormatterMode mode) => mode.HasLineBreakForced;

            public override FormatterMode LeftSideWithLineBreaksMode
                (FormatterMode mode) => FormatterMode.ForcedLineBreaks;
        }

        sealed class LeftParenthesisFormatter : Formatter
        {
            public override IndentDirection IndentRightSite => IndentDirection.ToRight;
            public override bool UseLineBreakBeforeToken(FormatterMode mode) => mode.HasLineBreakForced;
            public override bool UseLineBreakAfterToken(FormatterMode mode) => true;

            public override FormatterMode RightSideWithLineBreaksMode
                (FormatterMode mode) => FormatterMode.ForcedLineBreaks;

            public override FormatterMode LeftSideWithLineBreaksMode(FormatterMode mode)
            {
                NotImplementedFunction(mode);
                return base.LeftSideWithLineBreaksMode(mode);
            }
        }

        sealed class ColonFormatter : Formatter
        {
            public override IndentDirection IndentLeftSite => IndentDirection.ToLeft;

            public override bool UseLineBreakBeforeToken(FormatterMode mode)
            {
                NotImplementedFunction(mode);
                return base.UseLineBreakBeforeToken(mode);
            }

            public override bool UseLineBreakAfterToken(FormatterMode mode) => true;

            public override FormatterMode LeftSideWithLineBreaksMode(FormatterMode mode) => FormatterMode.None;

            public override FormatterMode RightSideWithLineBreaksMode(FormatterMode mode)
            {
                NotImplementedFunction(mode);
                return base.RightSideWithLineBreaksMode(mode);
            }
        }

        sealed class ChainFormatter : Formatter
        {
            public override IndentDirection IndentTokenAndRightSite => IndentDirection.ToRight;

            public override FormatterMode LeftSideWithLineBreaksMode(FormatterMode mode)
                => mode;

            public override FormatterMode RightSideWithLineBreaksMode(FormatterMode mode)
                => mode;
        }

        abstract class AnyListFormatter : Formatter
        {
            public override bool UseLineBreakAfterToken(FormatterMode mode) => mode.HasLineBreakForced;
        }

        sealed class ListFormatter : AnyListFormatter
        {
            public override FormatterMode RightSideWithLineBreaksMode
                (FormatterMode mode) => FormatterMode.ForcedLineBreaks;
        }

        sealed class LastListFormatter : AnyListFormatter {}

        static readonly Formatter Empty = new EmptyFormatter();
        static readonly Formatter Default = new DefaultFormatter();
        static readonly Formatter RightParenthesis = new RightParenthesisFormatter();
        static readonly Formatter LeftParenthesis = new LeftParenthesisFormatter();
        static readonly Formatter Colon = new ColonFormatter();
        static readonly Formatter Chain = new ChainFormatter();
        static readonly Formatter List = new ListFormatter();
        static readonly Formatter LastList = new LastListFormatter();

        public static Formatter CreateFormatter(Syntax syntax)
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

        public virtual IndentDirection IndentTokenAndRightSite => IndentDirection.NoIndent;
        public virtual IndentDirection IndentLeftSite => IndentDirection.NoIndent;
        public virtual IndentDirection IndentRightSite => IndentDirection.NoIndent;
        public virtual bool UseLineBreakBeforeToken(FormatterMode mode) => false;
        public virtual bool UseLineBreakAfterToken(FormatterMode mode) => false;
        public virtual FormatterMode LeftSideWithLineBreaksMode(FormatterMode mode) => FormatterMode.None;
        public virtual FormatterMode RightSideWithLineBreaksMode(FormatterMode mode) => FormatterMode.None;
    }
}