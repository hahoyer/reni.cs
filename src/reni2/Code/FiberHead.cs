using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Validation;

namespace Reni.Code
{
    abstract class FiberHead : CodeBase
    {
        static int _nextObjectId;

        protected FiberHead(int objectId)
            : base(objectId) { }

        protected FiberHead()
            : base(_nextObjectId++) { }

        protected virtual CodeBase TryToCombine(FiberItem subsequentElement) => null;

        internal override CodeBase Add(FiberItem subsequentElement)
        {
            var newResult = TryToCombine(subsequentElement);
            if(newResult == null)
                return new Fiber(this, subsequentElement);

            return newResult;
        }

        [DisableDump]
        internal override IEnumerable<Issue> Issues => Validation.Issue.Empty;

        [DisableDump]
        internal virtual bool IsNonFiberHeadList => false;
    }
}