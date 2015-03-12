using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Type;

namespace Reni.Validation
{
    sealed class IssueType : TypeBase
    {
        [EnableDump]
        readonly IssueBase _issue;

        public IssueType(IssueBase issue, Root rootContext)
        {
            _issue = issue;
            RootContext = rootContext;
        }

        [DisableDump]
        internal override Root RootContext { get; }

        [DisableDump]
        internal override bool Hllw => true;
        internal override string DumpPrintText => _issue.IssueId.Tag;

        internal Result IssueResult(Category category) => Result(category, Code);
        IssueType ConsequentialErrorType(IToken position) => _issue.ConsequentialError(position).Type(RootContext);

        CodeBase Code() => _issue.Code;

        internal sealed class ImplicitSearchResult
            : DumpableObject

        {
            [EnableDump]
            readonly IssueType _parent;

            public ImplicitSearchResult(IssueType parent) { _parent = parent; }
        }
    }
}