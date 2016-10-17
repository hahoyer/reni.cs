using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
{
    sealed class ScannerSyntaxError : CommonTokenType<Syntax>, ITokenClass, IValueProvider
    {
        readonly IssueId IssueId;

        public ScannerSyntaxError(IssueId issueId)
        {
            IssueId = issueId;
            StopByObjectIds(81);
        }

        string ITokenClass.Id => Id;

        public override string Id => "<error>";

        Result<Value> IValueProvider.Get(Syntax syntax)
        {
            if(syntax.Right == null)
            {
                var issues = IssueId.Create(syntax);
                return syntax.Left == null
                    ? new Result<Value>(new EmptyList(syntax), issues)
                    : syntax.Left.Value.With(issues);
            }

            NotImplementedMethod(syntax);
            return null;
        }

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
            => Syntax.CreateSourceSyntax(left, this, token, right);

    }
}