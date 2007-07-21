using System;
using HWClassLibrary.Debug;

namespace Reni.Code.ReplaceVisitor
{
    /// <summary>
    /// Handle argument replaces
    /// </summary>
    public abstract class ReplaceArg : Base
    {
        readonly Code.Base _actualArg;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ReplaceArg"/> class.
        /// </summary>
        /// <param name="actualArg">The actual.</param>
        /// created 28.09.2006 22:52
        public ReplaceArg(Code.Base actualArg)
        {
            Tracer.Assert(actualArg != null, "actualArg != null");
            _actualArg = actualArg;
        }

        /// <summary>
        /// Gets the actual arg.
        /// </summary>
        /// <value>The actual arg.</value>
        /// created 28.09.2006 22:57
        [DumpData(false)]
        public Code.Base ActualArg { get { return _actualArg; } }
        /// <summary>
        /// Gets the actual.
        /// </summary>
        /// <value>The actual.</value>
        /// created 28.09.2006 22:46
        public abstract Code.Base Actual { get; }

        /// <summary>
        /// Contexts the ref.
        /// </summary>
        /// <param name="visitedObject">The visited object.</param>
        /// <returns></returns>
        /// created 17.10.2006 00:04
        public override Code.Base ContextRef<C>(ContextRef<C> visitedObject)
        {
            return null;
        }

        /// <summary>
        /// Args the specified visited object.
        /// </summary>
        /// <param name="visitedObject">The visited object.</param>
        /// <returns></returns>
        /// created 24.09.2006 20:17
        public override Code.Base Arg(Arg visitedObject)
        {
            visitedObject.StopByObjectId(363);
            Tracer.Assert(Actual.Size == visitedObject.Size, "Actual=" + Actual.Dump() + "\nvisitedObject=" + visitedObject.Dump());
            return Actual;
        }
    }
}