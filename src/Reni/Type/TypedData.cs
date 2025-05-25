using Reni.Basics;

namespace Reni.Type;

sealed class TypedData : DumpableObject
{
    readonly TypeBase Type;
    BitsConst Data;

    [DisableDump]
    public string ToText
    {
        get
        {
            var converter = Type.GetConversionToText();


            NotImplementedMethod();
            return default!;
        }
    }

    [DisableDump]
    public object Value
    {
        get
        {
            NotImplementedMethod();
            return default!;
        }
    }

    public TypedData(TypeBase type, BitsConst data)
    {
        Type = type;
        Data = data;
    }
}
