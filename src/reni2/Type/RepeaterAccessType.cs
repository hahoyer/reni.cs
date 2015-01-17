using System;
using System.Collections.Generic;
using System.Linq;
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
        IRepeaterType RepeaterType { get; }

        internal RepeaterAccessType(IRepeaterType repeaterType) { RepeaterType = repeaterType; }

        [DisableDump]
        internal override bool Hllw => false;

        protected override bool IsMutable => RepeaterType.IsMutable;

        internal override TypeBase ValueType => RepeaterType.ElementType;

        protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize + RepeaterType.IndexSize;

        protected override CodeBase SetterCode()
            => Pair(ValueType.SmartPointer)
                .ArgCode
                .ArrayAssignment(ValueType.Size, RepeaterType.IndexSize);

        protected override CodeBase GetterCode() => ArgCode.ArrayAccess(ValueType.Size, RepeaterType.IndexSize);
    }
}