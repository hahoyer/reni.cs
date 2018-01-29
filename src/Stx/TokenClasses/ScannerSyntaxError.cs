using hw.Parser;

namespace Stx.TokenClasses
{
    sealed class ScannerSyntaxError : ParserTokenType<Syntax>
    {
        readonly IssueId IssueId;

        public ScannerSyntaxError(IssueId issueId) => IssueId = issueId;

        public override string Id => "<error>";

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        protected override string GetNodeDump() => base.GetNodeDump() + ":" + IssueId;
    }
}