using Bnf.Forms;
using Bnf.Scanner;
using hw.DebugFormatter;
using hw.Parser;

namespace Bnf.TokenClasses
{
    [BelongsTo(typeof(TokenFactory))]
    sealed class Define : TokenClass
    {
        public const string TokenId = "::=";

        [DisableDump]
        public override string Id => TokenId;

        protected override IForm GetForm(Syntax parent)
        {
            var left = parent.Left.Form.Checked<Forms.Define.IDestination>(parent);
            if(left is IError)
                return left;

            var right = parent.Right.Form.Checked<IExpression>(parent);
            if(right is IError)
                return right;

            return new Forms.Define(parent, (Forms.Define.IDestination) left, (IExpression) right);
        }
    }

    [BelongsTo(typeof(TokenFactory))]
    sealed class DefineSpecial : TokenClass
    {
        public const string TokenId = "::=?";

        [DisableDump]
        public override string Id => TokenId;

        protected override IForm GetForm(Syntax parent)
        {
            var left = parent.Left.Form.Checked<Forms.Define.IDestination>(parent);
            if(left is IError)
                return left;

            if(parent.Right != null)
                return new Error(parent, IssueId.SyntaxError);

            return new Forms.Define(parent, (Forms.Define.IDestination) left, null);
        }
    }

}