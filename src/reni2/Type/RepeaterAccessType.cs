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
                .ArraySetter(ValueType.Size, RepeaterType.IndexSize);

        protected override CodeBase GetterCode() => ArrayGetter;
        internal CodeBase ArrayGetter => ArgCode.ArrayGetter(ValueType.Size, RepeaterType.IndexSize);

        internal Result AccessResult(Category category, TypeBase left, TypeBase right)
        {
            var rightType = RootContext.BitType.Number(RepeaterType.IndexSize.ToInt());

            var rightResult = right
                .Conversion(category.Typed, rightType)
                .DereferencedAlignedResult();

            return Result(category, left.PointerObjectResult(category) + rightResult);
        }
        internal Result PlusResult(Category category, ArrayReferenceType left, TypeBase right)
            => left
                .Result(category, Conversion(category, RepeaterType.ElementType.Pointer))
                .ReplaceArg(AccessResult(category, left, right));
    }
}