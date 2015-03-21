using System.Linq;
using System.Collections.Generic;
using System;
using Reni.Basics;
using Reni.Validation;

namespace Reni.Code
{
    sealed class IssueCode : CodeBase
    {
        static int _nextObjectId;
        readonly Issue[] _issues;

        internal IssueCode(params Issue[] issues)
            : base(_nextObjectId++)
        {
            _issues = issues;
        }
        protected override Size GetSize() => Size.Zero;
        internal override CodeBase Add(FiberItem subsequentElement)
        {
            throw new NotImplementedException();
        }
        internal override IEnumerable<Issue> Issues => _issues;
        internal override void Visit(IVisitor visitor) { }

        internal static IssueCode CheckedCreate(IEnumerable<CodeBase> data)
        {
            if(data == null || !data.Any())
                return null;

            return new IssueCode(data.SelectMany(item => ((IssueCode) item)._issues).ToArray());
        }
    }
}