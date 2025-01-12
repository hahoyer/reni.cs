using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [BelongsTo(typeof(DeclarationTokenFactory))]
    [Variant(1)]
    [Variant(2)]
    [Variant(3)]
    sealed class LeftParenthesis : TokenClass, IBelongingsMatcher, ILeftBracket
    {
        internal int Level { get; }

        public LeftParenthesis(int level) => Level = level;

        [DisableDump]
        public override string Id => TokenId(Level);

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => (otherMatcher as RightParenthesis)?.Level == Level;

        int IBracket.Level => Level;

        public static string TokenId(int level)
            => level == 0? PrioTable.BeginOfText : "{[(".Substring(level - 1, 1);
    }
}