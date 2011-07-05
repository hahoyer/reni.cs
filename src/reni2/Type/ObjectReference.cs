using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;

namespace Reni.Type
{
    internal sealed class ObjectReference : ReniObject, IReferenceInCode
    {
        private static int _nextObjectId;

        [EnableDump]
        private readonly TypeBase _objectType;

        [DisableDump]
        private readonly RefAlignParam _refAlignParam;

        internal ObjectReference(TypeBase objectType, RefAlignParam refAlignParam)
            : base(_nextObjectId++)
        {
            _objectType = objectType;
            _refAlignParam = refAlignParam;
            StopByObjectId(-1);
        }

        RefAlignParam IReferenceInCode.RefAlignParam { get { return _refAlignParam; } }
        internal override string DumpShort() { return base.DumpShort() + "(" + _objectType.DumpShort() + ")"; }

        internal Result Result(Category category)
        {
            return _objectType.SpawnAutomaticReference(_refAlignParam)
                .Result
                (category
                 , () => CodeBase.ReferenceCode(this)
                 , () => Refs.Create(this)
                );
        }

    }
}