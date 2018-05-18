using System.Collections.Generic;
using Bnf.Forms;
using hw.DebugFormatter;

namespace Bnf.TokenClasses
{
    sealed class Declarator : TokenType
    {
        public Declarator(string name)
        {
            Id = name;
        }

        [DisableDump]
        public override string Id {get;}

        protected override IForm GetForm(Syntax parent)
        {
            var left = parent.Left?.Form;
            var right = parent.Right?.Form;

            var userSymbol = new Forms.Declarator(parent, Id);
            if(left == null && right == null)
                return userSymbol;

            var expressions = new List<IExpression>();
            expressions.Add<Sequence, IExpression>(left);
            expressions.Add(userSymbol);
            expressions.Add<Sequence, IExpression>(right);

            return new Sequence(parent, expressions.ToArray());
        }
    }
}