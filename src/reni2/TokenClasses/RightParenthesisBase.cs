using hw.DebugFormatter;
using hw.Parser;

namespace Reni.TokenClasses
{
    [Variant(1)]
    [Variant(2)]
    [Variant(3)]
    abstract class RightParenthesisBase : TokenClass, IBelongingsMatcher, IRightBracket
    {
        internal int Level { get; }

        protected RightParenthesisBase(int level) => Level = level;

        [DisableDump]
        public override string Id => TokenId(Level);

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => (otherMatcher as LeftParenthesis)?.Level == Level;

        int IRightBracket.Level => Level;

        public static string TokenId(int level)
            => level == 0? PrioTable.EndOfText : "}])".Substring(level - 1, 1);
    }

    interface IRightBracket
    {
        int Level { get; }
    }

    interface ILeftBracket
    {
        int Level { get; }
    }
}