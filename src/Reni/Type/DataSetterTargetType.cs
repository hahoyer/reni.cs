using Reni.Basics;
using Reni.Code;
using Reni.Context;

namespace Reni.Type;

abstract class DataSetterTargetType : SetterTargetType
{
    protected abstract CodeBase GetSetterCode();
    protected abstract CodeBase GetGetterCode();

    protected override Result SetterResult(Category category)
        => Root.VoidType.GetResult(category, GetSetterCode);

    protected override Result GetterResult(Category category)
        => ValueType
            .ForcedPointer
            .GetResult(category, GetGetterCode);
}