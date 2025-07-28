using Reni.Struct;

namespace Reni.Type;

sealed class FunctionInstanceType : TagChild<TypeBase>
{
    public FunctionInstanceType(TypeBase parent)
        : base(parent) { }
    [DisableDump]
    protected override string TagTitle => "function_instance";

    internal override CompoundView FindRecentCompoundView() => Parent.FindRecentCompoundView();
}