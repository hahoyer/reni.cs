using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using Reni.Basics;
using Reni.Code;

namespace Reni.Type
{
    abstract class DataSetterTargetType : SetterTargetType
    {
        protected override Result SetterResult(Category category)
            => new Result(category, getCode: SetterCode);

        protected override Result GetterResult(Category category)
        {
            return ValueType
                .ForcedPointer
                .Result(category, GetterCode);
        }

        protected abstract CodeBase SetterCode();
        protected abstract CodeBase GetterCode();
    }
}