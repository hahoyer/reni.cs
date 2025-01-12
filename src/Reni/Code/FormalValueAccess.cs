using Reni.Basics;
using Reni.Struct;

namespace Reni.Code;

sealed class FormalValueAccess : DumpableObject
{
    readonly int Index;
    readonly int SizeValue;
    readonly IFormalValue FormalValue;
    int Size => SizeValue;

    public FormalValueAccess(IFormalValue formalValue, int index, int size)
    {
        FormalValue = formalValue;
        SizeValue = size;
        Index = index;
    }

    public static IFormalValue[] Transpose(FormalValueAccess[] accesses)
    {
        var distinctAccesses = accesses.Distinct().ToArray();
        if(distinctAccesses.Length == 1 && distinctAccesses[0] == null)
            return new IFormalValue[1];
        var result = accesses.Select(x => x == null? null : x.FormalValue).Distinct().ToArray();
        foreach(var formalValue in result)
            formalValue?.Check(accesses.Where(x => x != null && x.FormalValue == formalValue).ToArray());

        return result;
    }

    public new string Dump() => FormalValue.Dump(Index, SizeValue);

    public static IFormalValue BitsArray(BitsConst data) => new BitsArrayValue(data);

    public static IFormalValue Call
        (IFormalValue[] formalSubValues, FunctionId functionId) => new CallValue(formalSubValues, functionId);

    public static IFormalValue BitCast
        (IFormalValue formalSubValue, int castedBits) => new BitCastValue(formalSubValue, castedBits);

    public static IFormalValue Variable(char name) => new VariableValue(name);
    public static IFormalValue RefPlus(IFormalValue formalSubValue, int right) => formalSubValue.RefPlus(right);
    public static IFormalValue Dereference(IFormalValue formalSubValue) => new DereferenceValue(formalSubValue);

    public static IFormalValue BitArrayBinaryOp
        (string operation, IFormalValue formalLeftSubValue, IFormalValue formalRightSubValue)
        => new BitArrayBinaryOpValue(operation, formalLeftSubValue, formalRightSubValue);

    internal static void Check(FormalValueAccess[] accesses)
    {
        var ss = accesses.Select(x => x.Size).Distinct().ToArray();
        if(ss.Length != 1 || ss[0] != accesses.ToArray().Length)
            "Size problem".FlaggedLine();
        var ii = accesses.Select((x, i) => i - x.Index).Distinct().ToArray();
        if(ii.Length != 1 || ii[0] != 0)
            "Consequtivity problem".FlaggedLine();
    }
}

sealed class BitArrayBinaryOpValue : NamedValue
{
    readonly string Operation;
    readonly IFormalValue LeftSubValue;
    readonly IFormalValue RightSubValue;

    public BitArrayBinaryOpValue(string operation, IFormalValue leftSubValue, IFormalValue rightSubValue)
    {
        Operation = operation;
        LeftSubValue = leftSubValue;
        RightSubValue = rightSubValue;
    }

    protected override char GetNodeDump() => Operation[0];

    protected override string Dump
        (bool isRecursion) => "(" + LeftSubValue + ")" + Operation + "(" + RightSubValue + ")";
}

sealed class DereferenceValue : NamedValue
{
    readonly IFormalValue FormalSubValue;
    public DereferenceValue(IFormalValue formalSubValue) => FormalSubValue = formalSubValue;
    protected override char GetNodeDump() => 'd';

    protected override string Dump(bool isRecursion) => "(" + FormalSubValue.Dump() + ")d";
}

sealed class FormalPointer : NamedValue
{
    static int NextPointer;

    [EnableDump]
    readonly char Name;

    readonly FormalPointer[] Points;
    readonly int Index;

    FormalPointer(FormalPointer[] points, int index)
    {
        Name = FormalMachine.Names[NextPointer++];
        Points = points;
        Index = index;
    }

    protected override string Dump(bool isRecursion) => Name + " ";

    protected override IFormalValue RefPlus(int right)
    {
        Ensure(Points, Index + right);
        return Points[Index + right];
    }

    protected override char GetNodeDump() => Name;

    public static void Ensure(FormalPointer[] points, int index)
    {
        if(points[index] == null)
            points[index] = new(points, index);
    }
}

abstract class NamedValue : DumpableObject, IFormalValue
{
    void IFormalValue.Check(FormalValueAccess[] accesses) => FormalValueAccess.Check(accesses);

    string IFormalValue.Dump(int index, int size)
    {
        var text = CreateText(size);
        return text.Substring(index * 2, 2);
    }

    string IFormalValue.Dump() => Dump();

    IFormalValue IFormalValue.RefPlus(int right) => RefPlus(right);

    protected new abstract char GetNodeDump();

    protected abstract override string Dump(bool isRecursion);

    protected virtual IFormalValue RefPlus(int right)
    {
        NotImplementedMethod(right);
        return null;
    }

    string CreateText(int size)
    {
        var text = Dump();
        var fillCount = 2 * size - text.Length - 1;
        if(fillCount >= 0)
            return " " + "<".Repeat(fillCount / 2) + text + ">".Repeat(fillCount - fillCount / 2);
        return " " + (GetNodeDump() + "").Repeat(2 * size - 1);
    }
}

sealed class CallValue : NamedValue
{
    readonly IFormalValue[] FormalSubValues;
    readonly FunctionId FunctionId;

    public CallValue(IFormalValue[] formalSubValues, FunctionId functionId)
    {
        FormalSubValues = formalSubValues;
        FunctionId = functionId;
    }

    protected override char GetNodeDump() => 'F';

    protected override string Dump
        (bool isRecursion)
        => "F" + FunctionId + "(" + FormalSubValues.Aggregate("", (x, y) => (x == ""? "" : x + ", ") + y.Dump()) + ")";
}

sealed class VariableValue : NamedValue
{
    readonly char Name;
    bool IsPointer;
    public VariableValue(char name) => Name = name;

    protected override char GetNodeDump() => 'V';

    protected override string Dump(bool isRecursion)
    {
        var result = "V" + Name;
        if(IsPointer)
            result += "[p]";
        return result;
    }

    protected override IFormalValue RefPlus(int right)
    {
        IsPointer = true;
        return new PointerValue(this, right);
    }
}

sealed class PointerValue : NamedValue
{
    readonly VariableValue VariableValue;
    readonly int Right;

    public PointerValue(VariableValue variableValue, int right)
    {
        VariableValue = variableValue;
        Right = right;
    }

    protected override char GetNodeDump() => 'P';

    protected override string Dump(bool isRecursion) => VariableValue.Dump() + FormatRight();

    string FormatRight()
    {
        if(Right > 0)
            return "+" + Right;
        if(Right < 0)
            return "-" + -Right;
        return "";
    }
}

sealed class BitCastValue : DumpableObject, IFormalValue
{
    readonly IFormalValue FormalSubValue;
    readonly int CastedBits;

    public BitCastValue(IFormalValue formalSubValue, int castedBits)
    {
        FormalSubValue = formalSubValue;
        CastedBits = castedBits;
    }

    void IFormalValue.Check(FormalValueAccess[] accesses) => FormalValueAccess.Check(accesses);

    string IFormalValue.Dump(int index, int size)
    {
        if(index >= size - CastedBits)
            return " .";
        return FormalSubValue.Dump(index, size - CastedBits);
    }

    string IFormalValue.Dump() => FormalSubValue.Dump();

    IFormalValue IFormalValue.RefPlus(int right)
    {
        NotImplementedMethod(right);
        return null;
    }
}

sealed class BitsArrayValue : DumpableObject, IFormalValue
{
    readonly BitsConst Data;

    public BitsArrayValue(BitsConst data) => Data = data;
    void IFormalValue.Check(FormalValueAccess[] formalValueAccesses) { }

    string IFormalValue.Dump(int index, int size)
    {
        switch(Data.Access(Size.Create(index)))
        {
            case false:
                return " 0";
            case true:
                return " 1";
            default:
                return " .";
        }
    }

    IFormalValue IFormalValue.RefPlus(int right)
    {
        NotImplementedMethod(right);
        return null;
    }

    protected override string Dump(bool isRecursion) => Data.DumpValue();
}