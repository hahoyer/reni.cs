using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;
using Reni.SyntaxFactory;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(0)]
    [Variant(1)]
    [Variant(2)]
    sealed class List : TokenClass, IBelongingsMatcher, IStatementsToken
    {
        [DisableDump]
        internal readonly int Level;

        public List(int level) => Level = level;

        public override string Id => TokenId(Level);

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => otherMatcher == this;

        IStatementsProvider IStatementsToken.Provider => Factory.List;
        public static string TokenId(int level) => ",;.".Substring(level, 1);
    }
}