using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;

namespace Reni.Type
{
    sealed class RepeaterAccessType
        : DataSetterTargetType
    {
        [DisableDump]
        internal readonly IRepeaterType RepeaterType;

        internal RepeaterAccessType(IRepeaterType repeaterType) { RepeaterType = repeaterType; }

        [DisableDump]
        internal override bool Hllw { get { return false; } }
        internal override TypeBase ValueType { get { return RepeaterType.ElementType; } }

        protected override Size GetSize() { return Root.DefaultRefAlignParam.RefSize + RepeaterType.IndexSize; }

        protected override CodeBase SetterCode()
        {
            return Pair(ValueType.PointerKind)
                .ArgCode
                .ArrayAssignment(ValueType.Size, RepeaterType.IndexSize);
        }

        protected override CodeBase GetterCode() { return ArgCode.ArrayAccess(ValueType.Size, RepeaterType.IndexSize); }
    }
}