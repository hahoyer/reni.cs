using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Code;
using Reni.Context;
using Reni.Feature;

namespace Reni.Validation
{
    sealed class RootIssueType 
        : IssueType
    {
        public RootIssueType(Issue issue, Root root)
            : base(issue) { Root = root; }

        [DisableDump]
        internal override Root Root { get; }

        protected override CodeBase DumpPrintCode() => ArgCode.DumpPrintText(SimpleItemSize);
    }
}