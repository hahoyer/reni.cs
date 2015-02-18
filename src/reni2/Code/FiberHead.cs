using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
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
        internal override IEnumerable<IssueBase> Issues => IssueBase.Empty;

        [DisableDump]
        internal virtual bool IsNonFiberHeadList => false;
    }
}