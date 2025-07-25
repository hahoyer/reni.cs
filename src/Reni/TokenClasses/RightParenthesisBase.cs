using hw.Parser;
using Reni.TokenClasses.Brackets;

namespace Reni.TokenClasses
{
    [Variant(1)]
    [Variant(2)]
    [Variant(3)]
    abstract class RightParenthesisBase : TokenClass, IBelongingsMatcher, IRightBracket
    {
        internal int Level { get; }

        protected RightParenthesisBase(int level) => Level = level;

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => (otherMatcher as LeftParenthesis)?.Level == Level;

        int IBracket.Level => Level;

        Setup IBracket.Setup => Setup.Instances[Level];

        [DisableDump]
        public override string Id => ((IBracket)this).Setup.ClosingTokenId;
    }
}
