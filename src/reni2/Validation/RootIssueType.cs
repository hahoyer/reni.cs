using System;
using hw.DebugFormatter;
using Reni.Code;
using Reni.Context;

namespace Reni.Validation
{
    [Obsolete(message: "", error: true)]
    sealed class RootIssueType
        : IssueType
    {
        public RootIssueType(Issue issue, Root root)
            : base(issue) => Root = root;

        [DisableDump]
        internal override Root Root { get; }

        protected override CodeBase DumpPrintCode() => ArgCode.DumpPrintText(SimpleItemSize);
    }
}