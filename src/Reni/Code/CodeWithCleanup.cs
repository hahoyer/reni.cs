using hw.DebugFormatter;
using Reni.Basics;

namespace Reni.Code
{
    sealed class CodeWithCleanup : CodeBase
    {
        static int _nextObjectId;

        [EnableDump]
        readonly CodeBase CleanupCode;

        [EnableDump]
        readonly CodeBase Initialisation;

        internal CodeWithCleanup(CodeBase initialisation, CodeBase cleanupCode)
            : base(_nextObjectId++)
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

        internal override CodeBase ArrangeCleanupCode()
        {
            NotImplementedMethod();
            return null;
        }
    }
}