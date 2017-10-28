using System;
using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Basics;
using Reni.Code;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Validation
{
    [Obsolete("",true)]
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

        new readonly Issue Issue;

        protected IssueType(Issue issue) => Issue = issue;

        [DisableDump]
        internal override bool Hllw => throw new TryToAccessHllwException(this);

        internal override string DumpPrintText => Issue.IssueId.Tag;

        protected override TypeBase ReversePair(TypeBase first)
        {
            Tracer.Assert(!(first is IssueType));
            return new IssueInCompoundType(this, null);
        }

        internal override TypeBase Pair(TypeBase second, SourcePart position)
        {
            NotImplementedMethod(second);
            return base.Pair(second, position);
        }

        protected override string GetNodeDump() => base.GetNodeDump() + " " + DumpPrintText;

        protected IssueType CreateIssue(ISyntax currentTarget, IssueId issueId) 
            => new ConsequentialIssueType(this, currentTarget.Main);


    }

}