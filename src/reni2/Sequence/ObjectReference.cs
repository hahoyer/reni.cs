using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Type;

namespace Reni.Sequence
{
    [Obsolete("",true)]
    sealed class ObjectReference : DumpableObject, IContextReference
    {
        static int _nextObjectId;

        [EnableDump]
        readonly TypeBase _objectType;

        [DisableDump]
        readonly RefAlignParam _refAlignParam;
        readonly int _order;

        internal ObjectReference(TypeBase objectType, RefAlignParam refAlignParam)
            : base(_nextObjectId++)
        {
            _order = CodeArgs.NextOrder++;
            _objectType = objectType;
            _refAlignParam = refAlignParam;
            StopByObjectId(-1);
        }

        int IContextReference.Order { get { return _order; } }
        Size IContextReference.Size { get { return _refAlignParam.RefSize; } }
        protected override string GetNodeDump() { return base.GetNodeDump() + "(" + _objectType.NodeDump + ")"; }
        internal Result Result(Category category) { return _objectType.ReferenceInCode(category, this); }
    }
}