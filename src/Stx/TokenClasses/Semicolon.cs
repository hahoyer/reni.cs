using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Stx.CodeItems;
using Stx.Features;
using Stx.Forms;
using Stx.Scanner;

namespace Stx.TokenClasses
{
    [BelongsTo(typeof(TokenFactory))]
    sealed class Semicolon : TokenClass
    {
        public const string TokenId = ";";

        [DisableDump]
        public override string Id => TokenId;

        protected override IForm GetForm(Syntax parent)
        {
            var right = parent.Right;
            var left = parent.Left;
            var leftResult = left.GetResult(context);
            var rightResult = right.GetResult(context.Extend(leftResult.DataType));

            return new Result
            (
                parent.Token.Characters,
                getCodeItems:
                () => CodeItem
                    .Combine(leftResult.CodeItems, CodeItem.CreateSourceHint(parent.Token), rightResult.CodeItems)
                    .ToArray()
            );
        }
    }
}