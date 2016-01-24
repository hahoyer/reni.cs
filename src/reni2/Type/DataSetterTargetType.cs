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
            => Root.VoidType.Result(category, SetterCode);

        protected override Result GetterResult(Category category)
            => ValueType
                .ForcedReference
                .Type()
                .Result(category, GetterCode);

        protected abstract CodeBase SetterCode();
        protected abstract CodeBase GetterCode();
    }
}