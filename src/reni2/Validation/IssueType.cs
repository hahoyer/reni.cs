using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Type;

namespace Reni.Validation
{
    sealed class IssueType : TypeBase
    {
        [EnableDump]
        readonly Issue _issue;

        public IssueType(Issue issue, Root rootContext)
        {
            _issue = issue;
            RootContext = rootContext;
        }

        [DisableDump]
        internal override Root RootContext { get; }

        [DisableDump]
        internal override bool Hllw => true;
        internal override string DumpPrintText => _issue.IssueId.Tag;

        internal Result Result(Category category) => Result(category, Code);

        CodeBase Code() => _issue.Code;
    }
}