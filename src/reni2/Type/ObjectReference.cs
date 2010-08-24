using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;

namespace Reni.Type
{
    internal class ObjectReference: ReniObject, IReferenceInCode
    {
        private static int _nextObjectId;
        [IsDumpEnabled(true)]
        private readonly TypeBase _objectType;
        [IsDumpEnabled(false)]
        private readonly RefAlignParam _refAlignParam;

        public ObjectReference(TypeBase objectType, RefAlignParam refAlignParam)
            : base(_nextObjectId++)
        {
            _objectType = objectType;
            _refAlignParam = refAlignParam;
            StopByObjectId(-4);
        }

        RefAlignParam IReferenceInCode.RefAlignParam { get { return _refAlignParam; } }
        bool IReferenceInCode.IsChildOf(ContextBase contextBase) { return false; }
        string IReferenceInCode.Dump() { return "ObjectReference("+_objectType.DumpShort()+")"; }
    }
}