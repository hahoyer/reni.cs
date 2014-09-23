using System.Linq;
using System.Collections.Generic;
using System;
using Reni.Basics;
using Reni.Code;

namespace Reni.Type
{
    abstract class DataSetterTargetType : SetterTargetType
    {
        protected override Result SetterResult(Category category)
        {
            return new Result
                (
                category,
                getCode: SetterCode,
                getExts: CodeArgs.Arg
                );
        }

        protected override Result GetterResult(Category category)
        {
            return ValueType
                .PointerKind
                .Result(category, GetterCode);
        }

        protected abstract CodeBase SetterCode();
        protected abstract CodeBase GetterCode();
    }
}