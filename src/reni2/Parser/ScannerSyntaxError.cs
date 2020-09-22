using hw.Parser;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
{
    sealed class ScannerSyntaxError : ParserTokenType<Syntax>, ITokenClass, IValueProvider
    {
        readonly IssueId IssueId;

        public ScannerSyntaxError(IssueId issueId)
        {
            IssueId = issueId;
            StopByObjectIds(81);
        }

        string ITokenClass.Id => Id;

        Result<Value> IValueProvider.Get(Syntax syntax)
        {
            if(syntax.Right == null)
            {
                var issues = IssueId.Issue(syntax.Token.Characters);
                return syntax.Left == null
                    ? new Result<Value>(new EmptyList(syntax), issues)
                    : syntax.Left.Value.With(issues);
            }

            NotImplementedMethod(syntax);
            return null;
        }

        public override string Id => "<error>";

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
            => Syntax.Create(left, this, token, right);
    }
}