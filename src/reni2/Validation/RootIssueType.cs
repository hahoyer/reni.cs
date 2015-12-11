using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Context;

namespace Reni.Validation
{
    sealed class RootIssueType : IssueType
    {
        public RootIssueType(Issue issue, Root rootContext)
            : base(issue) { RootContext = rootContext; }

        [DisableDump]
        internal override Root RootContext { get; }
    }
}