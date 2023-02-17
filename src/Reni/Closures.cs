using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Type;

namespace Reni;

/// <summary>
///     Contains list of references to compiler environments.
/// </summary>
sealed class Closures : DumpableObject
{
    sealed class Closure : Singleton<Closure>, IContextReference
    {
        int IContextReference.Order => -1;
        protected override string GetNodeDump() => "Closure";
    }

    public static int NextOrder;
    static int NextId;

    internal readonly bool IsRecursive;

    [SmartNode]
    [DisableDump]
    readonly List<IContextReference> Data = new();

    readonly ValueCache<IContextReference[]> SortedDataCache;

    SizeArray SizesCache;

    [DisableDump]
    SizeArray Sizes => SizesCache ??= CalculateSizes();

    internal bool HasArguments => Contains(Closure.Instance);
    public int Count => Data.Count;

    [UsedImplicitly]
    IContextReference this[int i] => Data[i];

    public Size Size => Sizes.Size;
    public bool IsNone => Count == 0;
    IContextReference[] SortedData => SortedDataCache.Value;

    Closures(bool isRecursive)
        : base(NextId++)
    {
        IsRecursive = isRecursive;
        SortedDataCache = new(ObtainSortedData);
        StopByObjectIds(-10);
    }

    Closures(IContextReference context)
        : this(false)
        => Add(context);

    Closures(IEnumerable<IContextReference> a, IEnumerable<IContextReference> b)
        : this(false)
    {
        AddRange(a);
        AddRange(b);
    }

    Closures(IEnumerable<IContextReference> a)
        : this(false)
        => AddRange(a);

    protected override string GetNodeDump() => $"{base.GetNodeDump()}{(IsRecursive? "r" : "")}#{Count}";

    public override string DumpData()
    {
        var result = IsRecursive? "recursive/" : "";
        for(var i = 0; i < Count; i++)
        {
            if(i > 0)
                result += "\n";
            result += Tracer.Dump(Data[i]);
        }

        return result;
    }

    internal static Closures GetRecursivity() => new(true);
    internal static Closures GetVoid() => new(false);
    internal static Closures GetArgument() => new(Closure.Instance);


    void AddRange(IEnumerable<IContextReference> a)
    {
        foreach(var e in a)
            Add(e);
    }

    void Add(IContextReference e)
    {
        if(!Data.Contains(e))
            Data.Add(e);
    }


    public Closures Sequence(Closures closures)
        => closures.Count == 0? this :
            Count == 0? closures :
            new(Data, closures.Data);

    internal static Closures Create(IContextReference contextReference) => new(contextReference);

    SizeArray CalculateSizes()
    {
        var result = new SizeArray();
        for(var i = 0; i < Count; i++)
            result.Add(Data[i].Size());
        return result;
    }

    public Closures Without(IContextReference e)
    {
        if(!Data.Contains(e))
            return this;
        var r = new List<IContextReference>(Data);
        r.Remove(e);
        return new(r);
    }

    IContextReference[] ObtainSortedData()
        => Data
            .OrderBy(codeArg => codeArg.Order)
            .ToArray();

    public Closures WithoutArgument() => Without(Closure.Instance);

    Closures Without(Closures other)
        => other
            .Data
            .Aggregate(this, (current, refInCode) => current.Without(refInCode));

    public bool Contains(IContextReference context) => Data.Contains(context);

    public bool Contains(Closures other)
    {
        if(Count < other.Count)
            return false;

        for(int i = 0, j = 0; i < Count; i++)
        {
            var delta = SortedData[i].Order - other.SortedData[j].Order;

            if(delta > 0)
                return false;

            if(delta == 0)
            {
                j++;
                if(j == other.Count)
                    return true;
            }
        }

        return false;
    }

    public bool IsEqual(Closures other)
    {
        (IsRecursive == other.IsRecursive).Assert();

        if(Count != other.Count)
            return false;

        for(var i = 0; i < Count; i++)
            if(SortedData[i].Order != other.SortedData[i].Order)
                return false;

        return true;
    }

    internal CodeBase GetCode()
        => Data
            .Aggregate(CodeBase.Void, (current, t) => current + t.GetCode());

    internal CodeBase ReplaceRefsForFunctionBody(CodeBase code, CodeBase closureBase)
    {
        (!IsRecursive).Assert();
        var trace = ObjectId.In(59,509);
        StartMethodDump(trace, code, closureBase);
        try
        {
            var size = Root.DefaultRefAlignParam.RefSize;
            var closurePosition = closureBase.GetReferenceWithOffset(size * Data.Count);
            var result = code;
            foreach(var closure in Data)
            {
                Dump("closurePosition", closurePosition);
                BreakExecution();
                closurePosition = closurePosition.GetReferenceWithOffset(size * -1);
                result = result.ReplaceAbsolute(closure, () => closurePosition.GetDePointer(size));
                Dump("result", result);
            }

            return ReturnMethodDump(result);
        }
        finally
        {
            EndMethodDump();
        }
    }

    public static Closures operator +(Closures x, Closures y) => x.Sequence(y);
    public static Closures operator -(Closures x, Closures y) => x.Without(y);
    public static Closures operator -(Closures x, IContextReference y) => x.Without(y);
}