using Reni.Basics;
using Reni.Code;
using Reni.Context;

namespace Reni.Type;

abstract class DataSetterTargetType : SetterTargetType
{
    protected abstract CodeBase SetterCode();
    protected abstract CodeBase GetterCode();

    protected override Result SetterResult(Category category)
        => Root.VoidType.GetResult(category, SetterCode);

    protected override Result GetterResult(Category category)
        => ValueType
            .ForcedPointer
            .GetResult(category, GetterCode);
}