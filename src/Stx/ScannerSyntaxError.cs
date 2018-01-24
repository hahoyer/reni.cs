using System;
using hw.Parser;

namespace Stx {
    sealed class ScannerSyntaxError : CommonTokenType<Syntax>
    {
        readonly IssueId IssueId;

        public ScannerSyntaxError(IssueId issueId) => IssueId = issueId;
        public override string Id => "<error>";

        protected override Syntax Create
            (Syntax left, IToken token, Syntax right) => throw new NotImplementedException();
    }
}