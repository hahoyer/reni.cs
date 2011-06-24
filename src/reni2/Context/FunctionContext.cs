using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Code;

namespace Reni.Context
{
    internal sealed class FunctionContext : ReniObject, IReferenceInCode
    {
        internal FunctionContext(ContextBase parent, Function function) { throw new NotImplementedException(); }
        RefAlignParam IReferenceInCode.RefAlignParam { get { throw new NotImplementedException(); } }
    }
}