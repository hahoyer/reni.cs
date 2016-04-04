using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Code;
using Reni.Context;

namespace Reni.Validation
{
    sealed class ConsequentialIssueType : IssueType
    {
        readonly IssueType _issueType;

        public ConsequentialIssueType(IssueType issueType, SourcePart source)
            : base(new Issue(IssueId.ConsequentialError, source))
        {
            _issueType = issueType;
        }

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