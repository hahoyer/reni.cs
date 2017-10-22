using System;
using hw.DebugFormatter;
using Reni.Basics;
using Reni.Code;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Validation
{
    abstract class IssueType : TypeBase
    {
        class TryToAccessHllwException
            : Exception
        {
            readonly IssueType IssueType;

            public TryToAccessHllwException(IssueType issueType)
            {
                IssueType = issueType;
                Tracer.TraceBreak();
            }
        }

        [EnableDump]                                      
        internal readonly Issue Issue;

        protected IssueType(Issue issue) => Issue = issue;

        [DisableDump]
        internal override bool Hllw => throw new TryToAccessHllwException(this);

        internal override string DumpPrintText => Issue.IssueId.Tag;

        internal virtual CodeBase Code => Issue.Code;
        protected override string GetNodeDump() => base.GetNodeDump() + " " + DumpPrintText;

        protected override IssueType CreateIssue(ISyntax currentTarget, IssueId issueId)
            => new ConsequentialIssueType(this, currentTarget);

        internal Result Result(Category category) => Result(Filtered(category), () => Code);

        static Category Filtered(Category category) => category & (Category.Type | Category.Code | Category.Exts);
    }
}