using hw.DebugFormatter;

namespace Reni.FeatureTest.Helper;

sealed class TargetSetData : DumpableObject
{
    internal readonly string Target;
    internal readonly string Output;

    internal TargetSetData(string target, string output)
    {
        Target = target;
        Output = output ?? "";
    }
}

[AttributeUsage(AttributeTargets.Class)]
abstract class StringAttribute : Attribute
{
    internal readonly string Value;
    protected StringAttribute(string value) => Value = value;
}

[AttributeUsage(AttributeTargets.Class)]
sealed class OutputAttribute : StringAttribute
{
    internal OutputAttribute(string value)
        : base(value) { }
}

[AttributeUsage(AttributeTargets.Class)]
sealed class TargetAttribute : StringAttribute
{
    internal TargetAttribute(string value)
        : base(value) { }
}

[AttributeUsage(AttributeTargets.Class)]
sealed class InstanceCodeAttribute : StringAttribute
{
    public InstanceCodeAttribute(string value)
        : base(value) { }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
sealed class TargetSetAttribute : Attribute
{
    internal readonly TargetSetData TargetSet;

    internal TargetSetAttribute(string target, string output) => TargetSet = new(target, output);
}