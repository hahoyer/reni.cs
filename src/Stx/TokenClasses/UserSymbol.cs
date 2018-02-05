using hw.DebugFormatter;
using Stx.Forms;

namespace Stx.TokenClasses
{
    sealed class UserSymbol : TokenClass
    {
        public UserSymbol(string name) => Id = name;

        [DisableDump]
        public override string Id {get;}

        protected override IForm GetForm(Syntax parent)
        {
            Tracer.Assert(parent.Left == null, () => parent.Left.Dump());

            var right = parent.Right?.Form;

            switch(right)
            {
                case null: return new Forms.UserSymbol(parent, Id);
                case IIndex index: return new UserSymbolWithIndex(parent, Id, index.Value);
                case IExpression expression: return new UserSymbolWithExpression(parent, Id, expression);
            }

            return new Error<IForm>(parent, right);
        }
    }
}