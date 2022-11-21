using hw.DebugFormatter;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type;

abstract class Child<TParent>
    : TypeBase
        , IProxyType
        , IConversion
        , IChild<TParent>
    where TParent : TypeBase
{
    [Node]
    [DisableDump]
    internal readonly TParent Parent;

    protected Child(TParent parent) => Parent = parent;

    TParent IChild<TParent>.Parent => Parent;
    Result IConversion.Execute(Category category) => ParentConversionResult(category);
    TypeBase IConversion.Source => this;
    IConversion IProxyType.Converter => this;
    protected abstract Result ParentConversionResult(Category category);

    [DisableDump]
    internal override Root Root => Parent.Root;
}