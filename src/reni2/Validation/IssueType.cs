using System;
using hw.DebugFormatter;
using hw.Scanner;
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
        internal override Issue RecentIssue { get; }

        protected IssueType(Issue issue) => RecentIssue = issue;

        [DisableDump]
        internal override bool Hllw => throw new TryToAccessHllwException(this);

        internal override string DumpPrintText => RecentIssue.IssueId.Tag;

        internal virtual CodeBase Code => RecentIssue.Code;

        protected override TypeBase ReversePair(TypeBase first)
        {
            Tracer.Assert(!(first is IssueType));
            return new IssueInCompoundType(this);
        }

        internal override TypeBase Pair(TypeBase second)
        {
            NotImplementedMethod(second);
            return base.Pair(second);
        }

        protected override string GetNodeDump() => base.GetNodeDump() + " " + DumpPrintText;

        protected override IssueType CreateIssue(ISyntax currentTarget, IssueId issueId)
        {
            Tracer.Assert(issueId == IssueId.ConsequentialError);
            return new ConsequentialIssueType(this, currentTarget.Main);
        }

        internal Result Result(Category category) => Result(Filtered(category), () => Code);

        internal static Category Filtered
            (Category category) => category & (Category.Type | Category.Code | Category.Exts);

    }

}