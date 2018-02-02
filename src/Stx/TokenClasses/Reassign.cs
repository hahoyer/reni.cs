using hw.DebugFormatter;
using hw.Parser;
using Stx.Contexts;
using Stx.Features;
using Stx.Forms;
using Stx.Scanner;

namespace Stx.TokenClasses
{
    [BelongsTo(typeof(TokenFactory))]
    sealed class Reassign : TokenClass
    {
        public const string TokenId = ":=";

        [DisableDump]
        public override string Id => TokenId;

        protected override IForm GetForm(Syntax parent)
        {
            var left = parent.Left.Form.Checked<ReassignForm.IDestination>(parent);
            if(left is IError)
                return left;

            var right = parent.Right.Form.Checked<IExpression>(parent);
            if(right is IError)
                return right;

            return new ReassignForm(parent, (ReassignForm.IDestination) left, (IExpression) right);
        }
    }

    [BelongsTo(typeof(TokenFactory))]
    sealed class Colon : TokenClass
    {
        public const string TokenId = ":";

        [DisableDump]
        public override string Id => TokenId;

        protected override IForm GetForm(Syntax parent)
        {
            NotImplementedMethod(parent);
            return null;
        }
    }

    [BelongsTo(typeof(TokenFactory))]
    sealed class Of : TokenClass
    {
        public const string TokenId = "OF";

        [DisableDump]
        public override string Id => TokenId;

        protected override IForm GetForm(Syntax parent)
        {
            var left = parent.Left.Form.Checked<IExpression>(parent);
            if(left is IError)
                return left;

            var right = parent.Right.Form.Checked<Forms.Case.IItems>(parent);
            if(right is IError)
                return right;

            return new Forms.Case.Body(parent, (IExpression) left, (Forms.Case.IItems) right);

            NotImplementedMethod(parent);
            return null;
        }
    }

}