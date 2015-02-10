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
        internal RepeaterAccessType(IRepeaterType repeaterType)
        {
            IsMutable = repeaterType.IsMutable;
            ValueType = repeaterType.ElementType;
            IndexType = repeaterType.IndexType;
        }

        [DisableDump]
        TypeBase IndexType { get; }

        [DisableDump]
        protected override bool IsMutable { get; }

        [DisableDump]
        protected override TypeBase TargetType
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        [DisableDump]
        internal override TypeBase ValueType { get; }

        [DisableDump]
        internal override bool Hllw => false;

        protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize + IndexType.Size;

        protected override CodeBase SetterCode()
            => Pair(ValueType.SmartPointer)
                .ArgCode
                .ArraySetter(ValueType.Size, IndexType.Size);

        protected override CodeBase GetterCode() => ArrayGetter;
        CodeBase ArrayGetter => ArgCode.ArrayGetter(ValueType.Size, IndexType.Size);

        internal Result Result(Category category, Result leftResult, TypeBase right)
        {
            var rightResult = right
                .Conversion(category.Typed, IndexType)
                .DereferencedAlignedResult();

            return Result(category, leftResult + rightResult);
        }
    }
}