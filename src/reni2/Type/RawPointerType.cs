using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Struct;

namespace Reni.Type
{
    sealed class RawPointerType
        : TypeBase
    {
        readonly int _order;

        internal RawPointerType(TypeBase valueType)
        {
            _order = CodeArgs.NextOrder++;
            ValueType = valueType;
            Tracer.Assert(!valueType.Hllw, valueType.Dump);
            Tracer.Assert(!(valueType.CoreType is PointerType), valueType.Dump);
            StopByObjectId(-10);
        }

        [DisableDump]
        TypeBase ValueType { get; }

        [DisableDump]
        internal override Root RootContext => ValueType.RootContext;
        internal override string DumpPrintText => "(" + ValueType.DumpPrintText + ")~~~";
        [DisableDump]
        internal override bool Hllw => false;
        [DisableDump]
        internal override bool IsAligningPossible => false;

        protected override string GetNodeDump() => ValueType.NodeDump + "[RawPointer]";
        protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize;
    }
}