using System.Collections.Generic;
using Bnf.Forms;
using hw.Parser;

namespace Bnf.TokenClasses
{
    sealed class MatchedItem : ParserTokenType<Syntax>, ITokenClass
    {
        public MatchedItem(string id = "()") => Id = id;

        public override string Id {get;}

        IForm ITokenClass.GetForm(Syntax parent)
        {
            var left = parent.Left?.Form;
            var right = parent.Right?.Form;
            var expressions = new List<IExpression>();
            expressions.Add<Sequence, IExpression>(left);
            expressions.Add<Sequence, IExpression>(right);
            return new Sequence(parent, expressions.ToArray());
        }

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
            => right == null ? left : Syntax.Create(left, this, token, right);
    }

}