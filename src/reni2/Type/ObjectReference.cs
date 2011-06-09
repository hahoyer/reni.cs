﻿using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
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

        public ObjectReference(TypeBase objectType, RefAlignParam refAlignParam)
            : base(_nextObjectId++)
        {
            _objectType = objectType;
            _refAlignParam = refAlignParam;
            StopByObjectId(-1);
        }

        RefAlignParam IReferenceInCode.RefAlignParam { get { return _refAlignParam; } }
        bool IReferenceInCode.IsChildOf(ContextBase contextBase) { return false; }
        string IReferenceInCode.Dump() { return "ObjectReference(" + _objectType.DumpShort() + ")"; }
    }
}