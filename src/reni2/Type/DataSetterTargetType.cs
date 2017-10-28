using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Basics;
using Reni.Code;

namespace Reni.Type
{
    abstract class DataSetterTargetType : SetterTargetType
    {
        protected override Result SetterResult(Category category, SourcePart position)
            => Root.VoidType.Result(category, () => SetterCode(position));

        protected override Result GetterResult(Category category)
            => ValueType
                .ForcedPointer
                .Result(category, GetterCode);

        protected abstract CodeBase SetterCode(SourcePart position);
        protected abstract CodeBase GetterCode();

    }
}