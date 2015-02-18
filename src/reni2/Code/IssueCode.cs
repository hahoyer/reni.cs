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
        readonly IssueBase[] _issue;

        internal IssueCode(IssueBase issue)
            : base(_nextObjectId++)
        {
            _issue = new[] {issue};
        }
        protected override Size GetSize() => Size.Zero;
        internal override CodeBase Add(FiberItem subsequentElement)
        {
            throw new NotImplementedException();
        }
        internal override IEnumerable<IssueBase> Issues => _issue;
        internal override void Visit(IVisitor visitor) { }
    }
}