using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Stx.Forms;
using Stx.Scanner;

namespace Stx.TokenClasses
{
    [BelongsTo(typeof(TokenFactory))]
    sealed class Case : TokenClass
    {
        public const string TokenId = "CASE";

        [DisableDump]
        public override string Id => TokenId;

        protected override IForm GetForm(Syntax parent)
        {
            NotImplementedMethod(parent);
            return null;
        }
    }

    [BelongsTo(typeof(TokenFactory))]
    sealed class EndCase : TokenClass, IBracketMatch<Syntax>
    {
        internal interface IOf {}

        internal interface IElse {}

        public const string TokenId = "END_CASE";

        IParserTokenType<Syntax> IBracketMatch<Syntax>.Value {get;} = new MatchedItem();

        [DisableDump]
        public override string Id => TokenId;

        protected override IForm GetForm(Syntax parent)
        {
            var right = parent.Right;
            var left = parent.Left;
            Tracer.Assert(right == null);
            Tracer.Assert(left.TokenClass is Case);
            Tracer.Assert(left.Left == null);

            var center = left.Right;
            if(center == null)
                return new Error(parent, IssueId.SyntaxError);

            if(!(center.TokenClass is Of))
                return new Error(parent, IssueId.SyntaxError);

            var body = center.Right;
            if(body == null)
                return new Error(parent, IssueId.SyntaxError);

            if(!(body.TokenClass is Else))
                return new Error(parent, IssueId.SyntaxError);

            var clausesSyntax = body.Left;
            if(clausesSyntax == null)
                return new Error(parent, IssueId.SyntaxError);

            var elseSyntax = body.Right;
            if(elseSyntax == null)
                return new Error(parent, IssueId.SyntaxError);

            if(!(center.Left?.Form is IExpression value))
                return new Error(parent, IssueId.SyntaxError);

            var labeledClauses = clausesSyntax.CreateLabeledClauses();
            if(labeledClauses == null)
                return new Error(parent, IssueId.SyntaxError);

            if(!(elseSyntax.Form is IStatements elseStatements))
                return new Error(parent, IssueId.SyntaxError);

            var clasuses = labeledClauses
                .Concat(new[] {new Forms.Case.Clause(null, elseStatements.Data)})
                .ToArray();

            return new Forms.Case(parent, value, clasuses);
        }
    }
}