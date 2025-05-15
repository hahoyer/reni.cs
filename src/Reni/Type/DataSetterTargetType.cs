using Reni.Basics;
using Reni.Code;
using Reni.Context;

namespace Reni.Type;

abstract class DataSetterTargetType : SetterTargetType
{
    protected DataSetterTargetType(Root root)
        : base(root) { }

    protected abstract CodeBase GetSetterCode();
    protected abstract CodeBase GetGetterCode();

    protected override Result GetSetterResult(Category category)
        => Root.VoidType.GetResult(category, GetSetterCode);

    protected override Result GetGetterResult(Category category)
        => ValueType
            .ForcedPointer
            .GetResult(category, GetGetterCode);
}