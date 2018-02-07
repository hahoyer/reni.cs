using Bnf.Forms;
using Bnf.Scanner;
using hw.DebugFormatter;
using hw.Parser;

namespace Bnf.TokenClasses
{
    [BelongsTo(typeof(TokenFactory))]
    sealed class Reassign : TokenClass
    {
        public const string TokenId = ":=";

        [DisableDump]
        public override string Id => TokenId;

        protected override IForm GetForm(Syntax parent)
        {
            var left = parent.Left.Form.Checked<Forms.Reassign.IDestination>(parent);
            if(left is IError)
                return left;

            var right = parent.Right.Form.Checked<IExpression>(parent);
            if(right is IError)
                return right;

            return new Forms.Reassign(parent, (Forms.Reassign.IDestination) left, (IExpression) right);
        }
    }
}