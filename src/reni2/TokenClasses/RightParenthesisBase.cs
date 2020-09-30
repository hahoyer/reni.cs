using hw.DebugFormatter;
using hw.Parser;

namespace Reni.TokenClasses
{
    [Variant(1)]
    [Variant(2)]
    [Variant(3)]
    abstract class RightParenthesisBase : TokenClass, IBelongingsMatcher
    {
        protected RightParenthesisBase(int level) { Level = level; }

        [DisableDump]
        internal int Level { get; }
        [DisableDump]
        public override string Id => TokenId(Level);

        public static string TokenId(int level) => "}])".Substring(level - 1, 1);

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => (otherMatcher as LeftParenthesis)?.Level == Level;
    }
}