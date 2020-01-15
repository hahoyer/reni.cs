using hw.Helper;
using NUnit.Framework.Constraints;
using Reni.Feature;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    abstract class Formatter : EnumEx
    {
        sealed class RootFormatter : Formatter {}

        sealed class UnknownFormatter : Formatter {}

        sealed class SingleFormatter : Formatter {}

        sealed class ColonFormatter : Formatter
        {
            public override Context RightSideLineBreakContext(Context context) => context.BodyOfColon;
        }

        sealed class ChainFormatter : Formatter
        {
            public override int LineBreaksBeforeToken(Context context) => 1;
        }

        sealed class LeftParenthesisFormatter : Formatter
        {
            public override IndentDirection IndentRightSide => IndentDirection.ToRight;

            public override int LineBreaksBeforeToken(Context context) 
                => context.LineBreakBeforeLeftParenthesis ? 1 : 0;

            public override int LineBreaksAfterToken(Context context) => 1;
            public override Context RightSideLineBreakContext(Context context) => context.ForList;
            public override bool HasLineBreaksByContext(Context context) => context.LineBreaksForLeftParenthesis;
        }

        sealed class RightParenthesisFormatter : Formatter
        {
            public override int LineBreaksBeforeToken(Context context) => 0;
            public override Context LeftSideLineBreakContext(Context context) => context.LeftSideOfRightParenthesis;
            public override bool HasLineBreaksByContext(Context context) => context.LineBreaksForRightParenthesis;
        }

        sealed class ListFormatter : Formatter
        {
            public override int LineBreaksAfterToken(Context context) => 1;
            public override bool HasLineBreaksByContext(Context context) => context.LineBreaksForList;
            public override Context RightSideLineBreakContext(Context context) => context.ForList;
        }

        sealed class LastListFormatter : Formatter
        {
            public override int LineBreaksAfterToken(Context context) => 1;
            public override bool HasLineBreaksByContext(Context context) => context.LineBreaksForList;
        }

        sealed class ListEndFormatter : Formatter
        {
            public override int LineBreaksAfterToken(Context context) => 1;
            public override bool HasLineBreaksByContext(Context context) => context.LineBreaksForList;
            public override int LineBreaksRightOfRight => 1;
        }

        static readonly Formatter Root = new RootFormatter();
        static readonly Formatter List = new ListFormatter();
        static readonly Formatter ListEnd = new ListEndFormatter();
        static readonly Formatter LastList = new LastListFormatter();
        static readonly Formatter RightParenthesis = new RightParenthesisFormatter();
        static readonly Formatter LeftParenthesis = new LeftParenthesisFormatter();
        static readonly Formatter Colon = new ColonFormatter();
        static readonly Formatter Single = new SingleFormatter();
        static readonly Formatter Chain = new ChainFormatter();
        static readonly Formatter Unknown = new UnknownFormatter();


        public static Formatter CreateFormatter(Syntax syntax)
        {
            switch(syntax.TokenClass)
            {
                case BeginOfText _:
                case EndOfText _: return Root;
                case List _: return GetListTokenFormatter(syntax);
                case RightParenthesis _: return RightParenthesis;
                case LeftParenthesis _: return LeftParenthesis;
                case Colon _: return Colon;

                case ThenToken _:
                case Reni.TokenClasses.Function _:
                case ExclamationBoxToken _:
                case MutableDeclarationToken _:
                case ElseToken _: return Unknown;

                case ArgToken _:
                case Definable _:
                case Text _:
                case Number _:
                case TypeOperator _:
                case InstanceToken _: return syntax.Left != null ? Chain : Single;
            }

            NotImplementedFunction(syntax, "tokenClass", syntax.TokenClass);
            return default;
        }

        static Formatter GetListTokenFormatter(Syntax syntax)
            => syntax.Right == null
                ? LastList
                : syntax.Right.TokenClass == syntax.TokenClass
                    ? List
                    : ListEnd;

        public virtual IndentDirection IndentToken => IndentDirection.NoIndent;
        public virtual IndentDirection IndentLeftSide => IndentDirection.NoIndent;
        public virtual IndentDirection IndentRightSide => IndentDirection.NoIndent;

        public virtual int LineBreaksLeftOfLeft => 0;
        public virtual int LineBreaksBeforeToken(Context context) => 0;
        public virtual int LineBreaksAfterToken(Context context) => 0;
        public virtual Context LeftSideLineBreakContext(Context context) => context.None;
        public virtual Context RightSideLineBreakContext(Context context) => context.None;
        public virtual bool HasLineBreaksByContext(Context context) => false;
        public virtual int LineBreaksRightOfRight => 0;
        public virtual bool IsTrace => false;
    }
}