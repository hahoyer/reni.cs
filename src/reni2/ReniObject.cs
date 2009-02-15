using System;
using System.Diagnostics;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni
{
    [AdditionalNodeInfo("NodeDump"),Serializable]
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
        public virtual string NodeDump { get { return GetType().FullName + "." + ObjectId; } }
        [DumpData(false)]
        internal bool IsStopByObjectIdActive { get; private set; }

        public override string Dump()
        {
            return NodeDump + HWString.Surround(DumpData(), "{", "}");
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
            if(ObjectId == objectId)
                Tracer.ConditionalBreak(1, "", @"_objectId==" + objectId + "\n" + Dump());
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

        internal static bool IsObjectId(object syntax, int objectId) { return ((syntax is ReniObject) && ((ReniObject) syntax) .ObjectId == objectId); }
    }
}