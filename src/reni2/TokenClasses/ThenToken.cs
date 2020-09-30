using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ThenToken : InfixToken, IValueProvider
    {
        public const string TokenId = "then";
        public override string Id => TokenId;

        Result<Value> IValueProvider.Get(Syntax syntax)
            => CondSyntax.Create(syntax.Left, syntax.Right, null, syntax);
    }
}