using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.Basics;
using Reni.Code;
using Reni.Type;

namespace Reni.Validation
{
    abstract class IssueType : TypeBase
    {
        [EnableDump]
        readonly Issue _issue;

        protected IssueType(Issue issue) { _issue = issue; }

        [DisableDump]
        internal override bool Hllw => true;
        internal override string DumpPrintText => _issue.IssueId.Tag;

        internal override IssueType UndefinedSymbol(SourcePart source)
            => new ConsequentialIssueType(this, source);

        internal Result Result(Category category) => Result(category, Code);

        internal virtual CodeBase Code() => _issue.Code;
    }
}