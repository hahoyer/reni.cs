using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Basics;
using Reni.Code;
using Reni.Type;

namespace Reni.Validation
{
    abstract class IssueType : TypeBase
    {
        [EnableDump]
        internal readonly Issue Issue;

        protected IssueType(Issue issue) { Issue = issue; }

        [DisableDump]
        internal override bool Hllw => true;
        internal override string DumpPrintText => Issue.IssueId.Tag;
        protected override string GetNodeDump() => base.GetNodeDump() + " " + DumpPrintText;

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


        protected override IssueType CreateIssue(SourcePart source, IssueId issueId)
            => new ConsequentialIssueType(this, source);

        internal Result Result(Category category) => Result(category, () => Code);

        internal virtual CodeBase Code => Issue.Code;
    }
}