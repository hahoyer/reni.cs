using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Code;
using Reni.Context;
using Reni.TokenClasses;

namespace Reni.Validation
{
    sealed class ConsequentialIssueType : IssueType
    {
        readonly IssueType _issueType;

        public ConsequentialIssueType(IssueType issueType, ISyntax currentTarget)
            : base(new Issue(IssueId.ConsequentialError, currentTarget.Main)) { _issueType = issueType; }

        [DisableDump]
        internal override Root Root => _issueType.Root;

        internal override IEnumerable<string> DeclarationOptions
            => base.DeclarationOptions.Concat(InternalDeclarationOptions);

        IEnumerable<string> InternalDeclarationOptions
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }


        internal override CodeBase Code => _issueType.Code + base.Code;
    }
}