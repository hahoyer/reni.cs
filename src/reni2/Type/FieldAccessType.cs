using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Struct;

namespace Reni.Type
{
    sealed class FieldAccessType : DataSetterTargetType
    {
        internal FieldAccessType(CompoundView compoundView, int position)
        {
            View = compoundView;
            Position = position;
        }

        [EnableDump]
        public CompoundView View { get; }
        [EnableDump]
        public int Position { get; }

        protected override bool IsMutable => View.Compound.Syntax.IsReassignable(Position);
        protected override TypeBase TargetType => View.Type;
        internal override TypeBase ValueType => View.ValueType(Position);
        [DisableDump]
        internal override bool Hllw => false;
        Size FieldOffset => View.FieldOffset(Position);

        string GetCompoundIdentificationDump() => View.GetCompoundIdentificationDump() + ":" + Position;
        protected override string GetNodeDump() => base.GetNodeDump() + "(" + GetCompoundIdentificationDump() + ")";

        protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize;
        protected override CodeBase GetterCode() => ArgCode.ReferencePlus(FieldOffset);
        protected override CodeBase SetterCode() => Pair(ValueType.SmartPointer)
            .ArgCode
            .Assignment(ValueType.Size);

        internal override Result DestinationResult(Category category)
        {
            return base
                .DestinationResult(category)
                .AddToReference(() => FieldOffset);
        }

        internal override int? SmartArrayLength(TypeBase elementType) => ValueType.SmartArrayLength(elementType);
    }
}