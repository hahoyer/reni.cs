using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Basics;
using Reni.Validation;

namespace Reni.Code
{
    sealed class CodeWithCleanup : CodeBase
    {
        static int _nextObjectId;

        [EnableDump]
        readonly CodeBase Initialisation;
        [EnableDump]
        readonly CodeBase CleanupCode;

        internal CodeWithCleanup(CodeBase initialisation, CodeBase cleanupCode)
            :base(_nextObjectId++)
        {
            Initialisation = initialisation;
            CleanupCode = cleanupCode;
        }

        protected override Size GetSize() => Initialisation.Size;

        internal override CodeBase Add(FiberItem subsequentElement)
        {
            NotImplementedMethod(subsequentElement);
            return null;
        }

        [DisableDump]
        internal override IEnumerable<Issue> Issues => Validation.Issue.Empty;
    }
}