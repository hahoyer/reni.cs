using hw.DebugFormatter;
using hw.Parser;
using Stx.Contexts;
using Stx.Features;

namespace Stx.TokenClasses
{
    sealed class UserSymbol : TokenClass
    {
        public UserSymbol(string name) => Id = name;

        [DisableDump]
        public override string Id {get;}

        protected override Result GetResult(Context context, Syntax left, IToken token, Syntax right)
        {
            Tracer.Assert(left == null, () => left.Dump());

            var value = right?.GetResult(context.Subscription);
            var variable = context.Access(this, token, value?.DataType);
            return right == null ? variable : variable.Subscription(value);
        }
    }
}