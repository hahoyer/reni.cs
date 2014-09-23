using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Struct;

namespace Reni.Type
{
    sealed class FieldAccessType : DataSetterTargetType
    {
        [EnableDump]
        readonly Structure _structure;
        [EnableDump]
        readonly int _position;

        internal FieldAccessType(Structure structure, int position)
        {
            _structure = structure;
            _position = position;
        }

        internal override TypeBase ValueType { get { return _structure.ValueType(_position); } }

        [DisableDump]
        internal override bool Hllw { get { return false; } }

        [DisableDump]
        RefAlignParam RefAlignParam { get { return _structure.RefAlignParam; } }

        protected override string GetNodeDump() { return base.GetNodeDump() + "{" + _structure.NodeDump + "@" + _position + "}"; }

        protected override Size GetSize() { return RefAlignParam.RefSize; }

        Size FieldOffset { get { return _structure.FieldOffset(_position); } }

        protected override CodeBase GetterCode() { return ArgCode.ReferencePlus(FieldOffset); }

        protected override CodeBase SetterCode()
        {
            return
                Pair(ValueType.PointerKind)
                    .ArgCode
                    .Assignment(ValueType.Size);
        }

        internal override Result DestinationResult(Category category)
        {
            return base
                .DestinationResult(category)
                .AddToReference(() => FieldOffset);
        }

        internal override int? SmartSequenceLength(TypeBase elementType) { return ValueType.SmartSequenceLength(elementType); }

        internal override int? SmartArrayLength(TypeBase elementType) { return ValueType.SmartArrayLength(elementType); }
    }
}