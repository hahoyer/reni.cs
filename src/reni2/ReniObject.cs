using System.Diagnostics;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni
{
    public abstract class ReniObject : Dumpable
    {
        private static int _nextObjectId;
        private readonly int _objectId;

        protected ReniObject()
            : this(_nextObjectId++) {}

        protected ReniObject(int nextObjectId)
        {
            _objectId = nextObjectId;
        }

        [DumpData(false)]
        public virtual int ObjectId { get { return _objectId; } }
        [DumpData(false)]
        internal bool IsStopByObjectIdActive { get; private set; }

        public override string Dump()
        {
            return GetType().FullName + "." + ObjectId + HWString.Surround("{", DumpData(), "}");
        }

        public override string ToString()
        {
            return base.ToString() + " ObjectId=" + ObjectId;
        }

        public override string DebuggerDump()
        {
            return base.DebuggerDump() + " ObjectId=" + ObjectId;
        }

        [DebuggerHidden]
        public void StopByObjectId(int objectId)
        {
            var isStopByObjectIdActive = IsStopByObjectIdActive;
            IsStopByObjectIdActive = true;
            Tracer.ConditionalBreak(1, ObjectId == objectId, @"_objectId==" + objectId + "\n" + Dump());
            IsStopByObjectIdActive = isStopByObjectIdActive;
        }

        public bool DeepEqual(ReniObject other)
        {
            if(this == other)
                return true;
            if(GetType() != other.GetType())
                return false;
            var mi = GetType().GetMethod("TypedDeepEqual");
            if(mi != null)
                return (bool) mi.Invoke(this, new object[] {other});

            DumpMethodWithBreak("Not implemented", other);
            return false;
        }
    }
}