using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Basics;
using Reni.Code;

namespace Reni.Type
{
    abstract class DataSetterTargetType : SetterTargetType
    {
        protected override Result SetterResult(Category category)
            => Root.VoidType.Result(category, SetterCode);

        protected override Result GetterResult(Category category)
            => ValueType
                .ForcedPointer
                .Result(category, GetterCode);

        protected abstract CodeBase SetterCode();
        protected abstract CodeBase GetterCode();

    }
}