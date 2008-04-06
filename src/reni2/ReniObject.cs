using System.Diagnostics;
using HWClassLibrary.Debug;

namespace Reni
{
    /// <summary>
    /// defines common behaviour that cannot be found in MS-libs
    /// </summary>
    public abstract class ReniObject : Dumpable
    {
        private static int _nextObjectId;
        private readonly int _objectId;

        /// <summary>
        /// Initializes a new instance of the ReniObject class.
        /// </summary>
        /// created 15.09.2006 23:34
        protected ReniObject()
            : this(_nextObjectId++)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ReniObject class.
        /// </summary>
        /// <param name="nextObjectId">The next object id.</param>
        /// created 15.09.2006 23:34
        protected ReniObject(int nextObjectId)
        {
            _objectId = nextObjectId;
        }

        /// <summary>
        /// Object id for 
        /// </summary>
        public virtual int ObjectId
        {
            get { return _objectId; }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        /// created 09.02.2007 00:10
        public override string ToString()
        {
            return base.ToString() + " ObjectId=" + ObjectId;
        }

        /// <summary>
        /// generate dump string to be shown in debug windows
        /// </summary>
        /// <returns></returns>
        public override string DebuggerDump()
        {
            return base.DebuggerDump() + " ObjectId=" + ObjectId;
        }

        /// <summary>
        /// Use for debugging. Stop, when objectid matches
        /// </summary>
        /// <param name="objectId"></param>
        [DebuggerHidden]
        public void StopByObjectId(int objectId)
        {
            var isStopByObjectIdActive = IsStopByObjectIdActive;
            IsStopByObjectIdActive = true;
            Tracer.ConditionalBreak(1, ObjectId == objectId, @"_objectId==" + objectId + "\n" + Dump());
            IsStopByObjectIdActive = isStopByObjectIdActive;
        }

        [DumpData(false)]
        internal bool IsStopByObjectIdActive { get; private set; }

        /// <summary>
        /// Deep compare
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool DeepEqual(ReniObject other)
        {
            if (this == other)
                return true;
            if (GetType() != other.GetType())
                return false;
            var mi = GetType().GetMethod("TypedDeepEqual");
            if (mi != null)
                return (bool) mi.Invoke(this, new object[] {other});

            DumpMethodWithBreak("Not implemented", other);
            return false;
        }
    }
}