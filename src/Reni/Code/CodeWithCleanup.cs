using Reni.Basics;

namespace Reni.Code
{
    sealed class CodeWithCleanup : CodeBase
    {
        static int NextObjectId;

        [EnableDump]
        readonly CodeBase CleanupCode;

        [EnableDump]
        readonly CodeBase Initialisation;

        internal CodeWithCleanup(CodeBase initialisation, CodeBase cleanupCode)
            : base(NextObjectId++)
        {
            Initialisation = initialisation;
            CleanupCode = cleanupCode;
        }

        protected override Size GetSize() => Initialisation.Size;

        internal override CodeBase Concat(FiberItem subsequentElement)
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