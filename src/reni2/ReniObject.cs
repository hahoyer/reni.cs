using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;

namespace Reni
{
    [AdditionalNodeInfo("NodeDump"), Serializable]
    public abstract class ReniObject : Dumpable
    {
        private static int _nextObjectId;
        private readonly int _objectId;

        protected ReniObject()
            : this(_nextObjectId++) { }

        protected ReniObject(int nextObjectId) { _objectId = nextObjectId; }

        [DisableDump]
        public virtual int ObjectId { get { return _objectId; } }

        [DisableDump]
        public virtual string NodeDump { get { return DumpShort(); } }

        internal string DumpShort() { return GetType().FullName + "." + ObjectId; }

        [DisableDump]
        internal bool IsStopByObjectIdActive { get; private set; }

        protected override string Dump(bool isRecursion)
        {
            var result = NodeDump;
            if(!isRecursion)
                result += DumpData().Surround("{", "}");
            return result;
        }

        public override string ToString() { return base.ToString() + " ObjectId=" + ObjectId; }

        public override string DebuggerDump() { return base.DebuggerDump() + " ObjectId=" + ObjectId; }

        [DebuggerHidden]
        internal void StopByObjectId(int objectId) { StopByObjectId(1, objectId); }

        [DebuggerHidden]
        internal void StopByObjectId(int stackFrameDepth, int objectId)
        {
            var isStopByObjectIdActive = IsStopByObjectIdActive;
            IsStopByObjectIdActive = true;
            if(ObjectId == objectId)
                Tracer.ConditionalBreak(stackFrameDepth + 1, "", () => @"_objectId==" + objectId + "\n" + Dump());
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

    internal static class ReniObjectExtender
    {
        [DebuggerHidden]
        public static void StopByObjectId(this object t, int objectId)
        {
            var reniObject = t as ReniObject;
            if(reniObject == null)
                return;
            reniObject.StopByObjectId(1, objectId);
        }

        // will throw an exception if not a ReniObject
        internal static int GetObjectId(this object reniObject) { return ((ReniObject) reniObject).ObjectId; }
    }
}