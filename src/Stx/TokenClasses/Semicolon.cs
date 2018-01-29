using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Stx.CodeItems;
using Stx.Contexts;
using Stx.Features;
using Stx.Scanner;

namespace Stx.TokenClasses
{
    [BelongsTo(typeof(TokenFactory))]
    sealed class Semicolon : TokenClass
    {
        public const string TokenId = ";";

        [DisableDump]
        public override string Id => TokenId;

        protected override Result GetResult(Context context, Syntax left, IToken token, Syntax right)
        {
            var leftResult = left.GetResult(context);
            var rightResult = right.GetResult(context.Extend(leftResult.DataType));

            return new Result
            (
                token.Characters,
                getCodeItems:
                () => CodeItem
                    .Combine(leftResult.CodeItems, CodeItem.CreateSourceHint(token), rightResult.CodeItems)
                    .ToArray()
            );
        }
    }
}