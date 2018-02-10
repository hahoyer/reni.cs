using System.Collections.Generic;
using Bnf.Forms;
using Bnf.Scanner;
using hw.DebugFormatter;
using hw.Parser;

namespace Bnf.TokenClasses
{
    [BelongsTo(typeof(TokenFactory))]
    sealed class Semicolon : TokenType
    {
        public const string TokenId = ";";

        [DisableDump]
        public override string Id => TokenId;

        protected override IForm GetForm(Syntax parent)
        {
            var left = parent.Left?.Form;
            var right = parent.Right?.Form;
            var expressions = new List<IStatement>();
            expressions.Add<Statements, IStatement>(left);
            expressions.Add<Statements, IStatement>(right);
            return new Statements(parent, expressions.ToArray());
        }
    }
}