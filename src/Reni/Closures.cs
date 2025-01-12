using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Type;

namespace Reni;

/// <summary>
///     Contains list of references to compiler environments.
/// </summary>
sealed class Closures : DumpableObject, IEquatable<Closures>
{
    sealed class ArgumentsClosure : Singleton<ArgumentsClosure>, IContextReference
    {
        int IContextReference.Order => -1;
        protected override string GetNodeDump() => "Closure";
    }

    public static int NextOrder;
    static int NextId;

    readonly bool IsRecursive;

    [SmartNode]
    [DisableDump]
    readonly List<IContextReference> Data = new();

    readonly ValueCache<IContextReference[]> SortedDataCache;

    SizeArray SizesCache;

    [DisableDump]
    SizeArray Sizes => SizesCache ??= CalculateSizes();

    internal bool HasArguments => Contains(ArgumentsClosure.Instance);
    public int Count => Data.Count;

    [UsedImplicitly]
    IContextReference this[int i] => Data[i];

    public Size Size => Sizes.Size;
    public bool IsNone => Count == 0;
    IContextReference[] SortedData => SortedDataCache.Value;

    Closures(bool isRecursive = false)
        : base(NextId++)
    {
        IsRecursive = isRecursive;
        SortedDataCache = new(ObtainSortedData);
        StopByObjectIds(-497, -509);
    }

    Closures(IContextReference context)
        : this()
        => Add(context);

    Closures(IEnumerable<IContextReference> a, IEnumerable<IContextReference> b)
        : this()
    {
        AddRange(a);
        AddRange(b);
    }

    Closures(IEnumerable<IContextReference> a)
        : this()
        => AddRange(a);

    public bool Equals(Closures other)
        => other != null && Data.SequenceEqual(other.Data);

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

    public override bool Equals(object obj)
        => ReferenceEquals(this, obj) || (obj is Closures other && Equals(other));

    public override int GetHashCode() => Data.Sum(item => item.Order);

    internal static Closures GetRecursivity() => new(true);
    internal static Closures GetVoid() => new();
    internal static Closures GetArgument() => new(ArgumentsClosure.Instance);


    void AddRange(IEnumerable<IContextReference> a)
    {
        foreach(var e in a)
            Add(e);
    }

    void Add(IContextReference newItem)
    {
        var index = Data.IndexWhere(item=>item.Order >= newItem.Order);

        if(index == null)
        {
            Data.Add(newItem);
            return;
        }

        index.AssertIsNotNull();
        if(Data[index.Value] == newItem)
            return;

        (index == Data.Count || Data[index.Value].Order > newItem.Order)
            .Assert(()=>$"{Data[index.Value].NodeDump()}=={newItem.NodeDump()}:{Data[index.Value].Order}");
        Data.Insert(index.Value, newItem);
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

    public Closures WithoutArgument() => Without(ArgumentsClosure.Instance);

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
        var trace = ObjectId.In(-59, -509);
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
    public static bool operator ==(Closures left, Closures right) => Equals(left, right);
    public static bool operator !=(Closures left, Closures right) => !Equals(left, right);
}