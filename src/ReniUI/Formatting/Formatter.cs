using hw.Helper;
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
            public override bool HasLineBreaksAfterToken(Context context) => true;
        }

        sealed class ChainFormatter : Formatter
        {
            public override bool HasLineBreaksBeforeToken(Context context) => true;
        }

        sealed class LeftParenthesisFormatter : Formatter
        {
            public override IndentDirection IndentRightSide => IndentDirection.ToRight;

            public override bool HasLineBreaksBeforeToken(Context context)
                => context.LineBreakBeforeLeftParenthesis;

            public override bool HasLineBreaksAfterToken(Context context) => true;
            public override bool HasLineBreaksRightOfAll => true;
            public override Context RightSideLineBreakContext(Context context) => context.ForList;
            public override bool HasLineBreaksByContext(Context context) => context.LineBreaksForLeftParenthesis;
        }

        sealed class RightParenthesisFormatter : Formatter
        {
            public override Context LeftSideLineBreakContext(Context context) => context.LeftSideOfRightParenthesis;
            public override bool HasLineBreaksByContext(Context context) => context.LineBreaksForRightParenthesis;
        }

        abstract class ListItemFormatter : Formatter
        {
            public sealed override bool HasLineBreaksAfterToken(Context context) => true;
            public sealed override bool HasLineBreaksByContext(Context context) => context.LineBreaksForList;
            public sealed override bool HasMultipleLineBreaksOnRightSide(Context context) => context.HasMultipleLineBreaksOnRightSide;

        }

        sealed class ListFormatter : ListItemFormatter 
        {
            public override Context RightSideLineBreakContext(Context context) => context.ForList;

            public override Context BothSideContext(Context context, Syntax syntax) 
                => context.MultiLineBreaksForList(syntax.Left, syntax.Right?.Left);
        }

        sealed class LastListFormatter : ListItemFormatter 
        {
        }

        sealed class ListEndFormatter : ListItemFormatter 
        {
            public override Context BothSideContext(Context context, Syntax syntax) 
                => context.MultiLineBreaksForList(syntax.Left, syntax.Right);
        }

        public static readonly Formatter Root = new RootFormatter();
        public static readonly Formatter List = new ListFormatter();
        public static readonly Formatter ListEnd = new ListEndFormatter();
        public static readonly Formatter LastList = new LastListFormatter();
        public static readonly Formatter RightParenthesis = new RightParenthesisFormatter();
        public static readonly Formatter LeftParenthesis = new LeftParenthesisFormatter();
        public static readonly Formatter Colon = new ColonFormatter();
        public static readonly Formatter Single = new SingleFormatter();
        public static readonly Formatter Chain = new ChainFormatter();
        public static readonly Formatter Unknown = new UnknownFormatter();


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
                case InstanceToken _: return syntax.Left == null ? Single : Chain;
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

        public virtual bool HasLineBreaksLeftOfLeft => false;
        public virtual bool HasLineBreaksRightOfRight => false;
        public virtual bool HasLineBreaksRightOfAll => false;
        public virtual bool HasLineBreaksBeforeToken(Context context) => false;
        public virtual bool HasLineBreaksAfterToken(Context context) => false;
        public virtual Context LeftSideLineBreakContext(Context context) => context.None;
        public virtual Context RightSideLineBreakContext(Context context) => context.None;
        public virtual Context BothSideContext(Context context, Syntax syntax) => context.None;
        public virtual bool HasLineBreaksByContext(Context context) => false;
        public virtual bool IsTrace => false;
        public virtual bool HasMultipleLineBreaksOnRightSide(Context context) => false;
    }
}