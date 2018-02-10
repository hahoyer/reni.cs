using System.Collections.Generic;
using System.Linq;
using Bnf.Forms;
using hw.DebugFormatter;
using hw.Scanner;

namespace Bnf.TokenClasses
{
    sealed class StringLiteral : TokenType, IFactoryTokenType
    {
        readonly char Delimiter;

        public StringLiteral(char delimiter) => Delimiter = delimiter;

        hw.Scanner.ITokenType ITokenTypeFactory.Get(string id) => this;
        public override string Id => "<Literal>";

        protected override IForm GetForm(Syntax parent)
        {
            var left = parent.Left?.Form;
            var literal = new Literal(parent, Parse(parent.Token.Characters.Id));
            var right = parent.Right?.Form;

            if(left == null && right == null)
                return literal;

            var expressions = new List<IExpression>();
            expressions.Add<Sequence, IExpression>(left);
            expressions.Add(literal);
            expressions.Add<Sequence, IExpression>(right);

            return new Sequence(parent, expressions.ToArray());
        }

        string Parse(string text)
        {
            Tracer.Assert(text.Length >= 2);
            Tracer.Assert(text.First() == Delimiter);
            Tracer.Assert(text.Last() == Delimiter);
            var result = "";
            for(var i = 1; i < text.Length - 1; i++)
            {
                if(i == Scanner.Lexer.StringEscapeChar)
                    i++;
                result += text[i];
            }

            return result;
        }
    }
}