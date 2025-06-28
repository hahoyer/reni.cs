using Reni.Basics;

namespace Reni.Type;

sealed class TypedData : DumpableObject
{
    readonly TypeBase Type;
    readonly BitsConst Data;

    [DisableDump]
    internal object Value => Type.GetDataValue(Data);

    internal TypedData(TypeBase type, BitsConst data)
    {
        Type = type;
        Data = data;
    }
}
