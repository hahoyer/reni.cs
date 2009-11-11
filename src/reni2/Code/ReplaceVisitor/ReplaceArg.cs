using HWClassLibrary.Debug;

namespace Reni.Code.ReplaceVisitor
{
    /// <summary>
    /// Handle argument replaces
    /// </summary>
    internal abstract class ReplaceArg : Base
    {
        private readonly CodeBase _actualArg;

        internal ReplaceArg(CodeBase actualArg)
        {
            Tracer.Assert(actualArg != null, "actualArg != null");
            _actualArg = actualArg;
        }

        [DumpData(false)]
        protected CodeBase ActualArg { get { return _actualArg; } }

        protected abstract CodeBase Actual { get; }

        internal override CodeBase Arg(Arg visitedObject)
        {
            visitedObject.StopByObjectId(-363);
            Tracer.Assert(Actual.Size == visitedObject.Size,
                "Actual=" + Actual.Dump() + "\nvisitedObject=" + visitedObject.Dump());
            return Actual;
        }
    }
}